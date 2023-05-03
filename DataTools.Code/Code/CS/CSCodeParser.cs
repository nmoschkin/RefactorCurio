using DataTools.Code.Markers;
using DataTools.Text;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataTools.Code.CS
{
    /// <summary>
    /// C# Code Parser Class
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/></typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
    internal class CSCodeParser<TMarker, TList> : CodeParserBase<TMarker, TList>
        where TMarker : IMarker<TMarker, TList>, new()
        where TList : class, IMarkerList<TMarker>, new()
    {
        public override TList GetMarkersForCommit()
        {
            return markers;
        }

        /// <summary>
        /// Load and optionally parse a C# code file.
        /// </summary>
        /// <param name="filename">The valid relative or absolute pathname to the source file.</param>
        /// <param name="lazy">True to load the file without parsing it.</param>
        /// <exception cref="FileNotFoundException"></exception>
        public override void LoadFile(string filename, bool lazy = false)
        {
            lock (SyncRoot)
            {
                if (!File.Exists(filename)) throw new FileNotFoundException(filename);

                lastErrors = new List<string>();
                Filename = filename;
                OutputPath = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "\\");
                IsLazyLoad = lazy;

                if (!lazy) Parse(File.ReadAllText(filename));
            }
        }

        /// <summary>
        /// Create a new empty code parser.
        /// </summary>
        protected CSCodeParser()
        {
        }

        /// <summary>
        /// Create a new code parser from the given source code text.
        /// </summary>
        /// <param name="text">The source code text to parse.</param>
        public CSCodeParser(string text)
        {
            Parse(text);
        }

        /// <summary>
        /// Load and optionally parse a C# code file.
        /// </summary>
        /// <param name="filename">The valid relative or absolute pathname to the source file.</param>
        /// <param name="lazy">True to load the file without parsing it.</param>
        /// <returns>A new <see cref="CSCodeParser{TMarker, TList}"/> or derived object.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static CSCodeParser<TMarker, TList> LoadFromFile(string filename, bool lazy = false)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            var res = new CSCodeParser<TMarker, TList>();
            res.LoadFile(filename, lazy);
            return res;
        }

        public override int LoadMarkerText(IMarker marker)
        {
            if (string.IsNullOrEmpty(marker.Content))
            {
                string text;

                if (string.IsNullOrEmpty(this.Text))
                {
                    text = File.ReadAllText(Filename);
                }
                else
                {
                    text = this.Text;
                }

                marker.Content = text.Substring(marker.StartPos, marker.EndPos - marker.StartPos - 1);
            }

            return marker.Content?.Length ?? 0;
        }

        /// <summary>
        /// Parse the given C# source code text.
        /// </summary>
        /// <param name="text">The C# source code text to parse.</param>
        /// <returns>True if successful.</returns>
        protected override bool Parse(string text)
        {
            lock (SyncRoot)
            {
                ParseSuccess = false;

                this.text = text;
                lines = text.Replace("\r", "").Split('\n');

                try
                {
                    markers = InternalParseCSCode(text.ToCharArray());

                    var cf = new AtomicGenerationInfo<TMarker, TList>()
                    {
                        PreambleBegin = 0,
                        PreambleEnd = preambleTo,
                        Markers = markers ?? new TList()
                    };

                    mfile = cf;

                    foreach (var marker in markers)
                    {
                        SetAtomicFile(marker, cf);
                    }

                    SetParent(markers, default);

                    ParseSuccess = true;
                    IsLazyLoad = false;
                }
                catch (SyntaxErrorException ex)
                {
                    lastErrors.Add(ex.Message);
                    IsLazyLoad = true;
                }

                lines = null;
                this.text = null;

                return ParseSuccess;
            }
        }

        private void SetParent(TList markers, TMarker parent, string parentPath = null)
        {
            string pp = parentPath ?? "";

            if (parent != null && !string.IsNullOrEmpty(parent.Name) && parent.Kind != MarkerKind.Namespace)
            {
                if (!string.IsNullOrEmpty(pp)) pp = pp + "." + parent.Name;
                else pp = parent.Name;
            }

            foreach (var marker in markers)
            {
                marker.ParentElementPath = pp;
                marker.ParentElement = parent;

                if (marker.Children != null && marker.Children.Count > 0)
                {
                    SetParent(marker.Children, marker, pp);
                }
            }
        }

        #region CSharp Code Parsing Internals

        protected static Dictionary<MarkerKind, Regex> patterns = new Dictionary<MarkerKind, Regex>();
        protected static Regex genericPatt;

        /// <summary>
        /// The big list of all C# keywords and intrinsic types
        /// </summary>
        private static readonly string[] FilterType1 = new string[] {
            "public", "private", "protected", "internal", "global",
            "class", "const", "struct", "record", "interface", "namespace", "delegate", "event", "enum", "readonly",
            "async", "extern", "override", "abstract", "sealed", "new", "unsafe", "fixed", "static", "using", "get", "set",
            "for", "while", "do", "if", "else", "switch", "case", "default", "break", "yield", "return", "add", "remove",
            "throw", "try", "catch", "finally",
            "foreach", "in", "out", "ref", "null", "is", "not",
            "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "partial",
            "float", "double", "decimal",
            "Guid", "DateTime",
            "string", "char", "this", "base",
            "void", "var", "dynamic", "object",
            "Enum", "Tuple", "bool", "true", "false", "nint", "IntPtr", "UIntPtr"
        };

        /// <summary>
        /// A list of keywords to process for the <see cref="TypeAndMethodParse(string, TMarker)"/> (in literally no particular order.)
        /// </summary>
        private static readonly string[] FilterType2 = new string[] {
            "global", "ref", "sealed", "class", "interface", "record", "struct", "partial",
            "namespace", "public", "private", "static", "async", "abstract", "const",
            "readonly", "unsafe", "fixed", "delegate", "event", "virtual", "protected",
            "extern",
            "internal", "override", "new", "using", "get", "set", "add", "remove", "enum" };

        static CSCodeParser()
        {
            //patterns.Add(MarkerKind.Using, new Regex(@"using (.+)\s*;"));
            //patterns.Add(MarkerKind.Namespace, new Regex(@"namespace (.+)"));
            //patterns.Add(MarkerKind.This, new Regex(@".*(this)\s*\[.+\].*"));
            //patterns.Add(MarkerKind.Class, new Regex(@".*class\s+([A-Za-z0-9_@.]+).*"));
            //patterns.Add(MarkerKind.Interface, new Regex(@".*interface\s+([A-Za-z0-9_@.]+).*"));
            //patterns.Add(MarkerKind.Struct, new Regex(@".*struct\s+([A-Za-z0-9_@.]+).*"));
            //patterns.Add(MarkerKind.Enum, new Regex(@".*enum\s+([A-Za-z0-9_@.]+).*"));
            //patterns.Add(MarkerKind.Record, new Regex(@".*record\s+([A-Za-z0-9_@.]+).*"));
            //patterns.Add(MarkerKind.Delegate, new Regex(@".*delegate\s+.+\s+([A-Za-z0-9_@.]+)\(.*\)\s*;"));
            //patterns.Add(MarkerKind.Event, new Regex(@".*event\s+.+\s+([A-Za-z0-9_@.]+)\s*"));
            //patterns.Add(MarkerKind.Const, new Regex(@".*const\s+.+\s+([A-Za-z0-9_@.]+)\s*"));
            patterns.Add(MarkerKind.Operator, new Regex(@".*operator\s+(\S+)\(.*\)"));
            //patterns.Add(MarkerKind.ForLoop, new Regex(@"\s*for\s*\(.*;.*;.*\)"));
            //patterns.Add(MarkerKind.DoWhile, new Regex(@"\s*while\s*\(.*\)\s*;"));
            //patterns.Add(MarkerKind.While, new Regex(@"\s*while\s*\(.*\)"));
            //patterns.Add(MarkerKind.Switch, new Regex(@"\s*switch\s*\(.+\)"));
            //patterns.Add(MarkerKind.Case, new Regex(@"\s*case\s*\(.+\)\s*:"));
            //patterns.Add(MarkerKind.UsingBlock, new Regex(@"\s*using\s*\(.*\)"));
            //patterns.Add(MarkerKind.Lock, new Regex(@"\s*lock\s*\(.*\)"));
            //patterns.Add(MarkerKind.Unsafe, new Regex(@"\s*unsafe\s*$"));
            //patterns.Add(MarkerKind.Fixed, new Regex(@"\s*fixed\s*"));
            //patterns.Add(MarkerKind.ForEach, new Regex(@"\s*foreach\s*\(.*\)"));
            //patterns.Add(MarkerKind.Do, new Regex(@"\s*do\s*(\(.+\)|$)"));
            //patterns.Add(MarkerKind.Else, new Regex(@"\s*else\s*.*"));
            //patterns.Add(MarkerKind.ElseIf, new Regex(@"\s*else if\s*(\(.+\)|$)"));
            //patterns.Add(MarkerKind.If, new Regex(@"\s*if\s*(\(.+\)|$)"));
            //patterns.Add(MarkerKind.Get, new Regex(@"\s*get\s*($|\=\>).*"));
            //patterns.Add(MarkerKind.Set, new Regex(@"\s*set\s*($|\=\>).*"));
            //patterns.Add(MarkerKind.Add, new Regex(@"\s*add\s*($|\=\>).*"));
            //patterns.Add(MarkerKind.Remove, new Regex(@"\s*remove\s*($|\=\>).*"));
            //patterns.Add(MarkerKind.FieldValue, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*\=.+;$"));
            //patterns.Add(MarkerKind.Method, new Regex(@".* ([A-Za-z0-9_@.]+).*\s*\(.*\)\s*(;|\=\>|$|\s*where\s*.+:.+)"));
            patterns.Add(MarkerKind.EnumValue, new Regex(@"\s*([A-Za-z0-9_@.]+)(\s*=\s*(.+))?[,]?"));
            //patterns.Add(MarkerKind.Property, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*($|\=\>).*"));
            //patterns.Add(MarkerKind.Field, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*;$"));

            genericPatt = new Regex(@".* ([A-Za-z0-9_@.]+)\s*<(.+)>.*");
        }

        /// <summary>
        /// The actual C# parser engine.
        /// </summary>
        /// <param name="chars">The character array to interpret.</param>
        /// <returns>A list of parsed markers.</returns>
        /// <remarks>
        /// Only override this if you really know what you're doing!
        /// </remarks>
        protected virtual TList InternalParseCSCode(char[] chars)
        {
            lock (SyncRoot)
            {
                int i, j, c = chars.Length;

                TList markers = new TList();
                var rootlist = markers;
                TMarker currMarker = default;
                string prevWord = "";
                var strack = new Stack<MarkerKind>();
                var stack = new Stack<TMarker>();
                var listStack = new Stack<TList>();

                StringBuilder currWord = new StringBuilder();
                StringBuilder forScan = new StringBuilder();
                StringBuilder sb;

                forScan.Capacity = chars.Length;
                int inif = 0;
                int gbstart = 0;
                int startPos = 0;
                int scanStartPos = 0;

                int startLine = 0;

                int currLine = 1;

                int currLevel = 0;

                string currNS = "";

                int pre = -1;

                bool parenclo = false;

                Regex currCons = null;
                Regex currDecons = null;

                string currName = null;
                MarkerKind currPatt = MarkerKind.Code;

                List<string> attrs = null;
                bool zapbody = false;

                unrecognizedWords.Clear();

                for (i = 0; i < c; i++)
                {
                    if (chars[i] == '\r' || chars[i] == '\n')
                    {
                        forScan.Append(chars[i]);
                    }
                    else
                    {
                        if (inif > 0 && chars[i] != '#') continue;

                        if (AllowedChar(chars[i], true))
                        {
                            currWord.Append(chars[i]);
                        }
                        else if (currWord.Length > 0)
                        {
                            var cw = currWord.ToString();

                            if (prevWord == "" || prevWord == "new")
                            {
                                if (!FilterType1.Contains(cw))
                                {
                                    if (!unrecognizedWords.Contains(cw))
                                    {
                                        unrecognizedWords.Add(cw);
                                    }

                                    if (currMarker != null)
                                    {
                                        if (currMarker.UnknownWords == null)
                                        {
                                            currMarker.UnknownWords = new List<string>();
                                        }
                                        if (!currMarker.UnknownWords.Contains(cw))
                                        {
                                            currMarker.UnknownWords.Add(cw);
                                        }
                                    }
                                }
                            }

                            prevWord = cw;
                            currWord.Clear();
                        }

                        forScan.Append(chars[i]);
                    }

                    if (chars[i] == '\n')
                    {
                        currLine++;
                    }
                    else if (chars[i] == '\'')
                    {
                        TextTools.QuoteFromHere(chars, i, ref currLine, out int? spt, out int? ept, quoteChar: '\'', withQuotes: true);
                        i = (int)ept;
                        prevWord = "";
                    }
                    else if (chars[i] == '\"')
                    {
                        TextTools.QuoteFromHere(chars, i, ref currLine, out int? spt, out int? ept, withQuotes: true);
                        i = (int)ept;
                        prevWord = "";
                    }
                    else if (chars[i] == '(')
                    {
                        parenclo = true;
                        prevWord = "";
                    }
                    else if (chars[i] == '[' && !parenclo)
                    {
                        prevWord = "";

                        var sl = currLine;
                        var lookahead = TextTools.TextBetween(chars, i, ref currLine, '[', ']', out int? spt, out int? ept, withDelimiters: true);

                        if (lookahead == null) continue;

                        if (!Regex.IsMatch(lookahead, @"\[\s*\w|_|@[\w\d._@]+\s*\(?.*?\)?\]"))
                        {
                            continue;
                        }

                        forScan.Remove(forScan.Length - 1, 1);

                        if (attrs == null) attrs = new List<string>();
                        var spa = TextTools.Split(lookahead.Substring(1, lookahead.Length - 2), ",");
                        foreach (var r in spa)
                        {
                            var attr = FirstNameFromHere(r, true);
                            if (!FilterType1.Contains(attr) && !unrecognizedWords.Contains(attr))
                            {
                                unrecognizedWords.Add(attr);

                                if (currMarker != null)
                                {
                                    if (currMarker.UnknownWords == null)
                                    {
                                        currMarker.UnknownWords = new List<string>();
                                    }

                                    if (!currMarker.UnknownWords.Contains(attr))
                                    {
                                        currMarker.UnknownWords.Add(attr);
                                    }
                                }
                            }
                        }

                        attrs.AddRange(spa);

                        i = (int)ept;
                        scanStartPos = i + 1;
                    }
                    else if ((chars[i] == ';' && !zapbody) && currPatt == MarkerKind.Property)
                    {
                        var fs = forScan.ToString().Replace(";", "").Trim();

                        StripAccessModifiers(fs, out var amres, out var pros);

                        while (startPos < c - 1 && char.IsWhiteSpace(chars[startPos]))
                        {
                            startPos++;
                        }

                        if (pros == "get" || pros == "set")
                        {
                            var tmark = new TMarker
                            {
                                Namespace = currNS,
                                StartPos = startPos,
                                AccessModifiers = amres,
                                StartLine = currLine,
                                StartColumn = ColumnFromHere(chars, startPos),
                                ParentElementPath = currName,
                                EndPos = i - 1,
                                EndLine = currLine,
                                EndColumn = ColumnFromHere(chars, startPos + fs.Length),
                                Kind = pros == "set" ? MarkerKind.Set : MarkerKind.Get,
                                Name = pros,
                                Level = currLevel,
                                ScanHit = fs,
                            };

                            markers.Add(tmark);
                            forScan.Clear();
                        }
                        startPos = i + 1;
                    }
                    else if ((chars[i] == ';' && !zapbody) || (chars[i] == ',' && currPatt == MarkerKind.Enum))
                    {
                        parenclo = false;

                        if (currPatt == MarkerKind.Enum || (currPatt & MarkerKind.IsBlockLevel) != MarkerKind.IsBlockLevel)
                        {
                            var fs = forScan.ToString();
                            startLine += CountLines(fs);
                            fs = fs.Replace("\r", "").Replace("\n", "");
                            var lookback = TextTools.OneSpace(fs).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos + 1).Replace("\r", "").Replace("\n", "").Trim());

                            Match testEnum = null;

                            if (currPatt == MarkerKind.Enum)
                            {
                                testEnum = patterns[MarkerKind.EnumValue].Match(lookback);
                            }

                            if (testEnum != null && testEnum.Success)
                            {
                                currMarker = new TMarker
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    ParentElementPath = currName,
                                    EndPos = i - 1,
                                    EndLine = currLine,
                                    EndColumn = ColumnFromHere(chars, i - 1),
                                    Kind = MarkerKind.EnumValue,
                                    Name = testEnum.Groups[1].Value,
                                    Level = currLevel,
                                    ScanHit = lookback,
                                };
                            }
                            else
                            {
                                currMarker = new TMarker
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    ParentElementPath = currName,
                                    EndPos = i,
                                    EndLine = currLine,
                                    EndColumn = ColumnFromHere(chars, i),
                                    Level = currLevel,
                                    ScanHit = lookback,
                                    Attributes = attrs
                                };

                                TypeAndMethodParse(lookback, currMarker);

                                if (currMarker.IsExtern && currMarker.Attributes != null && currMarker.Attributes.Count > 0)
                                {
                                    var extattr = currMarker.Attributes.FirstOrDefault(x => x.StartsWith("DllImport"));
                                    if (!string.IsNullOrEmpty(extattr))
                                    {
                                        currMarker.ImportInfo = ImportInfo.Parse(extattr, currMarker.Name);
                                    }
                                }

                                if (currMarker.Kind == MarkerKind.Namespace)
                                {
                                    currNS = currMarker.Name;
                                    currMarker.Namespace = currMarker.Name;
                                }
                            }

                            markers.Add(currMarker);
                        }

                        if (i < c - 1)
                        {
                            for (i = i + 1; i < c; i++)
                            {
                                if ((char.IsWhiteSpace(chars[i]) || (chars[i] == '\r') || (chars[i] == '\n')))
                                {
                                    if (chars[i] == '\n') currLine++;
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }

                        scanStartPos = startPos = i + 1;
                        forScan.Clear();

                        prevWord = "";

                        startLine = currLine;
                        attrs = null;
                    }
                    else if (chars[i] == '{' || (chars[i] == '>' && i > 0 && chars[i - 1] == '='))
                    {
                        if (!zapbody)
                        {
                            zapbody = (chars[i] == '>' && i > 0 && chars[i - 1] == '=');

                            if (zapbody)
                            {
                                if (ContainsCode(currPatt))
                                {
                                    zapbody = false;
                                    continue;
                                }

                                forScan.Remove(forScan.Length - 2, 2);
                                gbstart = i - 1;
                            }
                            else
                            {
                                forScan.Remove(forScan.Length - 1, 1);
                            }
                        }

                        parenclo = false;
                        ++currLevel;

                        strack.Push(currPatt);

                        if (currPatt == MarkerKind.Event || currPatt == MarkerKind.Property || (currPatt & MarkerKind.IsBlockLevel) != MarkerKind.IsBlockLevel)
                        {
                            var fs = forScan.ToString();
                            startLine += CountLines(fs);
                            fs = fs.Replace("\r", "").Replace("\n", "");

                            var lookback = TextTools.OneSpace(fs).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos + 1).Replace("\r", "").Replace("\n", "").Trim());
                            Match cons = currCons?.Match(lookback) ?? null;
                            Match ops = patterns[MarkerKind.Operator].Match(lookback);

                            if (cons != null && cons.Success && !ops.Success)
                            {
                                currMarker = new TMarker
                                {
                                    StartPos = startPos,
                                    Namespace = currNS,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    Kind = MarkerKind.Constructor,
                                    ParentElementPath = currName,
                                    Name = currName,
                                    MethodParamsString = cons.Groups[1].Value,
                                    Level = currLevel,
                                    ScanHit = lookback,
                                    Attributes = attrs
                                };

                                currPatt = MarkerKind.Constructor;
                                markers.Add(currMarker);
                            }
                            else
                            {
                                cons = currDecons?.Match(lookback) ?? null;

                                if (cons != null && cons.Success)
                                {
                                    currMarker = new TMarker
                                    {
                                        Namespace = currNS,
                                        StartPos = startPos,
                                        StartLine = startLine,
                                        StartColumn = ColumnFromHere(chars, startPos),
                                        Kind = MarkerKind.Destructor,
                                        MethodParamsString = "()",
                                        Name = currName,
                                        Level = currLevel,
                                        ParentElementPath = currName,
                                        ScanHit = lookback,
                                        Attributes = attrs
                                    };

                                    currPatt = MarkerKind.Destructor;
                                    markers.Add(currMarker);
                                }
                                else
                                {
                                    currMarker = new TMarker
                                    {
                                        Namespace = currNS,
                                        StartPos = startPos,
                                        StartLine = startLine,
                                        StartColumn = ColumnFromHere(chars, startPos),
                                        Level = currLevel,
                                        ScanHit = lookback,
                                        ParentElementPath = currName,
                                        Attributes = attrs
                                    };

                                    TypeAndMethodParse(lookback, currMarker);

                                    currPatt = currMarker.Kind;

                                    if (currMarker.Kind == MarkerKind.Namespace)
                                    {
                                        currNS = currMarker.Name;
                                        currMarker.Namespace = currNS;
                                        if (pre == -1)
                                        {
                                            pre = currLine - 2;
                                        }
                                    }

                                    markers.Add(currMarker);

                                    if (currPatt == MarkerKind.Property && zapbody)
                                    {
                                        currMarker.Children = new TList();
                                        currMarker.Children.Add(new TMarker()
                                        {
                                            Kind = MarkerKind.Get,
                                            Name = "get",
                                            Namespace = currNS,
                                            StartPos = gbstart,
                                            StartLine = currLine,
                                            StartColumn = ColumnFromHere(chars, gbstart),
                                            ParentElementPath = currMarker.FullyQualifiedName
                                        });
                                    }

                                    if (currPatt == MarkerKind.Class || currPatt == MarkerKind.Struct || currPatt == MarkerKind.Record)
                                    {
                                        currName = currMarker.Name;

                                        currCons = new Regex($"^.*{currMarker.Name}\\s*(\\(.*\\)).*$");
                                        currDecons = new Regex($"^.*\\~{currMarker.Name}\\s*\\(\\)$");
                                    }
                                }
                            }
                        }

                        stack.Push(currMarker);
                        listStack.Push(markers);
                        markers = new TList();

                        if (i < c - 1)
                        {
                            for (i = i + 1; i < c; i++)
                            {
                                if ((char.IsWhiteSpace(chars[i]) || (chars[i] == '\r') || (chars[i] == '\n')))
                                {
                                    if (chars[i] == '\n') currLine++;
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }

                        scanStartPos = startPos = i + 1;

                        forScan.Clear();
                        prevWord = "";

                        startLine = currLine;
                        attrs = null;
                    }
                    else if (chars[i] == '}' || (chars[i] == ';' && zapbody))
                    {
                        parenclo = false;

                        if (currPatt == MarkerKind.Enum)
                        {
                            forScan.Remove(forScan.Length - 1, 1);
                            var fs = forScan.ToString();
                            startLine += CountLines(fs);
                            fs = fs.Replace("\r", "").Replace("\n", "");

                            var lookback = TextTools.OneSpace(fs).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos + 1).Replace("\r", "").Replace("\n", "").Trim());
                            var testEnum = patterns[MarkerKind.EnumValue].Match(lookback);

                            if (testEnum.Success)
                            {
                                currMarker = new TMarker
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
                                    ParentElementPath = currName,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    EndPos = i - 1,
                                    EndLine = currLine,
                                    EndColumn = ColumnFromHere(chars, i - 1),
                                    Kind = MarkerKind.EnumValue,
                                    Name = testEnum.Groups[1].Value,
                                    Level = currLevel,
                                    ScanHit = lookback,
                                };

                                markers.Add(currMarker);
                            }
                        }

                        if (zapbody && currMarker.Kind == MarkerKind.Property)
                        {
                            currMarker.Children[0].EndPos = i;
                            currMarker.Children[0].Name = "get";
                            currMarker.Children[0].EndLine = currLine;
                            currMarker.Children[0].EndColumn = ColumnFromHere(chars, i);
                            currMarker.Children[0].ScanHit = Text.Substring(gbstart, (i - gbstart) + 1);
                            currMarker.Children[0].Kind = MarkerKind.Get;

                            gbstart = 0;
                        }
                        
                        if (zapbody && chars[i] == ';') zapbody = false;

                        --currLevel;
                        currPatt = strack.Pop();
                        currMarker = stack.Pop();

                        if (currMarker != null)
                        {
                            currMarker.EndPos = i;
                            currMarker.EndLine = currLine;
                            currMarker.EndColumn = ColumnFromHere(chars, i);
                            if (currMarker.Children == null || currMarker.Children.Count == 0) currMarker.Children = markers;
                        }

                        markers = listStack.Pop();

                        //while (i < c - 1 && char.IsWhiteSpace(chars[i + 1]))
                        //{
                        //    i++;
                        //}

                        if (i < c - 1)
                        {
                            for (i = i + 1; i < c; i++)
                            {
                                if ((char.IsWhiteSpace(chars[i]) || (chars[i] == '\r') || (chars[i] == '\n')))
                                {
                                    if (chars[i] == '\n') currLine++;
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }
                        }

                        scanStartPos = startPos = i + 1;

                        forScan.Clear();
                        prevWord = "";

                        startLine = currLine;
                    }
                    else if (chars[i] == '#')
                    {
                        // this is quick and dirty.
                        // the logic is that if there's an #if block,
                        // it either won't terminate a bracket set inside,
                        // or the #else block will also terminate the bracket set.
                        // well, we only want one termination of a hypothetical bracket set.
                        // so we just want to be able to follow the flow of the document,
                        // we're not concerned with what's inside.

                        // TODO Follow compiler directives in real-time.

                        forScan.Remove(forScan.Length - 1, 1);
                        forScan.Append(' ');

                        parenclo = false;

                        currMarker = new TMarker()
                        {
                            StartColumn = ColumnFromHere(chars, i),
                            StartLine = currLine,
                            StartPos = i,
                            Level = currLevel,
                            ParentElementPath = currName,
                            Namespace = currNS
                        };

                        sb = new StringBuilder();

                        sb.Append(chars[i]);

                        for (j = i + 1; j < c; j++)
                        {
                            if (chars[j] != '\r' && chars[j] != '\n') sb.Append(chars[j]);

                            if (chars[j] == '\n')
                            {
                                forScan.Append('\n');

                                currMarker.EndColumn = ColumnFromHere(chars, j - 1);
                                currMarker.EndLine = currLine;
                                currMarker.EndPos = j - 1;
                                currMarker.Content = sb.ToString();
                                currMarker.Name = sb.ToString();
                                currMarker.Kind = MarkerKind.Directive;

                                markers.Add(currMarker);
                                currMarker = default;
                                currLine++;

                                break;
                            }
                        }

                        var dir = sb.ToString();

                        if (dir.StartsWith("#if "))
                        {
                            inif++;
                        }
                        else if (dir == "#endif" || dir == "#else")
                        {
                            inif--;
                            if (inif < 0) inif = 0;
                        }

                        if (j >= c) break;
                        i = j;
                    }
                    else if (i < c - 1 && chars[i] == '/' && chars[i + 1] == '/')
                    {
                        forScan.Remove(forScan.Length - 1, 1);
                        forScan.Append(' ');
                        parenclo = false;
                        currMarker = new TMarker()
                        {
                            StartColumn = ColumnFromHere(chars, i),
                            StartLine = currLine,
                            StartPos = i,
                            Level = currLevel,
                            ParentElementPath = currName,
                            Namespace = currNS
                        };

                        sb = new StringBuilder();

                        sb.Append(chars[i]);
                        sb.Append(chars[i + 1]);

                        bool docs = false;

                        for (j = i + 2; j < c; j++)
                        {
                            if (j == i + 2 && chars[j] == '/')
                            {
                                docs = true;
                            }

                            sb.Append(chars[j]);

                            if (chars[j] == '\n')
                            {
                                forScan.Append('\n');
                                currMarker.EndColumn = ColumnFromHere(chars, j - 1);
                                currMarker.EndLine = currLine;
                                currMarker.EndPos = j - 1;
                                currMarker.Content = sb.ToString();
                                currMarker.Kind = docs ? MarkerKind.XMLDoc : MarkerKind.LineComment;

                                markers.Add(currMarker);
                                currMarker = default;
                                currLine++;

                                break;
                            }
                        }

                        if (j >= c) break;
                        i = j;
                    }
                    else if (i < c - 3 && chars[i] == '/' && chars[i + 1] == '*')
                    {
                        forScan.Remove(forScan.Length - 1, 1);
                        forScan.Append(' ');

                        parenclo = false;
                        currMarker = new TMarker()
                        {
                            StartColumn = ColumnFromHere(chars, i),
                            StartLine = currLine,
                            StartPos = i,
                            Level = currLevel,
                            ParentElementPath = currName,
                            Namespace = currNS
                        };

                        sb = new StringBuilder();

                        sb.Append(chars[i]);
                        sb.Append(chars[i + 1]);

                        for (j = i + 2; j < c; j++)
                        {
                            sb.Append(chars[j]);

                            if (j < c - 1 && chars[j] == '*' && chars[j + 1] == '/')
                            {
                                sb.Append('/');

                                currMarker.EndColumn = ColumnFromHere(chars, j + 1);
                                currMarker.EndLine = currLine;
                                currMarker.EndPos = j + 1;
                                currMarker.Content = sb.ToString();
                                currMarker.Kind = MarkerKind.BlockComment;

                                markers.Add(currMarker);
                                currMarker = default;

                                scanStartPos = j + 2;
                                startLine = currLine;
                                break;
                            }
                            else if (chars[j] == '\n')
                            {
                                currLine++;
                                continue;
                            }
                        }

                        if (j >= c) break;
                        i = j;
                    }
                }

                preambleTo = pre;

                PostScanTasks(markers);

                return markers;
            }
        }

        private string FirstNameFromHere(string val, bool alsoDot)
        {
            val = val.Trim();
            var sb = new StringBuilder();

            int c = val.Length;
            for (int i = 0; i < c; i++)
            {
                if (AllowedChar(val[i], alsoDot))
                {
                    sb.Append(val[i]);
                }
                else
                {
                    break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parse the type, name and method parameters from a lookback string.
        /// </summary>
        /// <param name="lookback">The string to scan.</param>
        /// <param name="marker">The destination marker.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="SyntaxErrorException"></exception>
        private int TypeAndMethodParse(string lookback, TMarker marker)
        {
            if (string.IsNullOrEmpty(lookback)) return 0;

            int l = 0, i;
            var str = lookback;

            if (lookback[0] == '=') return 0;
            if (marker.Kind == MarkerKind.EnumValue) return 0;

            IList<string> filter2 = FilterType2;

            str = str.Trim();
            var w = str.ToCharArray();
            int c = w.Length;

            var tsb = new StringBuilder();
            var nsb = new StringBuilder();
            bool fl = false;
            bool lww = false;
            char ch = '\n';

            bool hasParams = false;
            bool eii = false;

            string generics1 = "";

            marker.Kind = MarkerKind.Code;

            for (i = 0; i < c; i++)
            {
                ch = w[i];

                if (AllowedChar(ch, true, types: true) && i < c - 1)
                {
                    if (fl) break;
                    tsb.Append(ch);
                    lww = true;
                }
                else if (ch == '<')
                {
                    try
                    {
                        var t = TextTools.TextBetween(w, i, ref l, '<', '>', out int? ax, out int? bx, withDelimiters: true);
                        if (t != null && bx != null)
                        {
                            i = (int)bx;
                            generics1 = t;
                            fl = true;
                        }
                        else
                        {
                            return 0;
                        }
                        lww = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else if (ch == '(')
                {
                    try
                    {
                        var t = TextTools.TextBetween(w, i, ref l, '(', ')', out int? ax, out int? bx, withDelimiters: true);

                        if (t != null && bx != null)
                        {
                            i = (int)bx;
                            fl = true;

                            tsb.Append(t);

                            var j = i + 1;
                            while (j < c - 1)
                            {
                                if (char.IsWhiteSpace(w[j]))
                                {
                                    j++;
                                    continue;
                                }
                                else if (w[j] == '?')
                                {
                                    tsb.Append('?');
                                    i = j;
                                    break;
                                }
                                else break;
                            }
                        }
                        else
                        {
                            return 0;
                        }

                        lww = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else if (ch == '[')
                {
                    try
                    {
                        var t = TextTools.TextBetween(w, i, ref l, '[', ']', out int? ax, out int? bx, withDelimiters: true);
                        if (t != null && bx != null)
                        {
                            i = (int)bx;
                            tsb.Append(t);
                            fl = true;
                        }
                        else
                        {
                            return 0;
                        }
                        lww = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else if (char.IsWhiteSpace(ch) || (AllowedChar(ch, true) && i == c - 1))
                {
                    if (!char.IsWhiteSpace(ch))
                    {
                        if (fl) break;
                        tsb.Append(ch);
                        lww = true;
                    }

                    if (lww)
                    {
                        var del = tsb.ToString();
                        if (filter2.Contains(del))
                        {
                            tsb.Clear();
                            lww = false;
                            switch (del)
                            {
                                case "public":
                                case "private":
                                case "internal":
                                case "protected":
                                case "global":
                                    marker.AccessModifiers |= (AccessModifiers)Enum.Parse(typeof(AccessModifiers), TextTools.TitleCase(del));
                                    break;

                                case "class":
                                case "event":
                                case "interface":
                                case "struct":
                                case "record":
                                case "const":
                                case "delegate":
                                case "get":
                                case "set":
                                case "add":
                                case "remove":
                                case "enum":
                                case "namespace":
                                case "using":
                                case "operator":
                                    marker.Kind = (MarkerKind)Enum.Parse(typeof(MarkerKind), TextTools.TitleCase(del));
                                    break;

                                case "async":
                                    marker.IsAsync = true;
                                    break;

                                case "abstract":
                                    marker.IsAbstract = true;
                                    break;

                                case "override":
                                    marker.IsOverride = true;
                                    break;

                                case "extern":
                                    marker.IsExtern = true;
                                    break;

                                case "new":
                                    marker.IsNew = true;
                                    break;

                                case "static":
                                    marker.IsStatic = true;
                                    break;

                                case "readonly":
                                    marker.IsReadOnly = true;
                                    break;

                                case "sealed":
                                    marker.IsSealed = true;
                                    break;

                                case "virtual":
                                    marker.IsVirtual = true;
                                    break;

                                case "ref":
                                    marker.IsRef = true;
                                    break;

                                case "unsafe":
                                    marker.IsUnsafe = true;
                                    break;

                                case "partial":
                                    marker.IsPartial = true;
                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                        {
                            i++;
                            break;
                        }
                    }
                    else
                    {
                        i++;
                        break;
                    }
                }
            }

            if (tsb.Length > 0)
            {
                if (marker.Kind == MarkerKind.Namespace)
                {
                    marker.Name = marker.Namespace = tsb.ToString();
                    return 1;
                }
                else
                {
                    switch (marker.Kind)
                    {
                        case MarkerKind.Class:
                        case MarkerKind.Struct:
                        case MarkerKind.Interface:
                        case MarkerKind.Record:
                        case MarkerKind.Enum:
                            marker.Name = tsb.ToString();
                            marker.Generics = generics1;
                            break;

                        case MarkerKind.Event:
                            marker.DataType = tsb.ToString();
                            marker.Generics = generics1;
                            break;

                        case MarkerKind.Using:
                            marker.Name = tsb.ToString();
                            return 1;

                        default:
                            tsb.Append(generics1);
                            marker.DataType = tsb.ToString();
                            break;
                    }
                }
            }
            else
            {
                switch (marker.Kind)
                {
                    case MarkerKind.Get:
                    case MarkerKind.Set:
                    case MarkerKind.Add:
                    case MarkerKind.Remove:
                        marker.Name = marker.Kind.ToString().ToLower();
                        break;

                    default:
                        return 0;
                }
            }

            tsb.Clear();

            for (; i < c; i++)
            {
                ch = w[i];

                if (AllowedChar(ch, true))
                {
                    //if (ch == '.') eii = true;
                    nsb.Append(ch);
                }
                else
                {
                    if (nsb.ToString() == "operator")
                    {
                        nsb.Clear();
                        i++;
                        while (i < c && w[i] != '(' && !char.IsWhiteSpace(w[i]))
                        {
                            nsb.Append(w[i]);
                            i++;
                        }
                        marker.Kind = MarkerKind.Operator;

                        if (marker.DataType == "explicit")
                        {
                            marker.IsExplicit = true;
                            marker.DataType = null;
                        }
                        else if (marker.DataType == "implicit")
                        {
                            marker.IsImplicit = true;
                            marker.DataType = null;
                        }
                    }

                    break;
                }
            }

            if (nsb.Length > 0)
            {
                marker.Name = nsb.ToString();
            }

            if (marker.DataType == null && marker.Kind == MarkerKind.Operator)
            {
                marker.DataType = marker.Name;
            }

            if (!string.IsNullOrEmpty(marker.DataType) && !FilterType1.Contains(marker.DataType))
            {
                if (marker.UnknownWords == null)
                {
                    marker.UnknownWords = new List<string>();
                }

                if (!marker.UnknownWords.Contains(marker.DataType))
                {
                    marker.UnknownWords.Add(marker.DataType);
                }
            }

            var retVal = 1;

            tsb.Clear();
            int x = -1;
            if (i < c)
            {
                for (; i < c; i++)
                {
                    ch = w[i];

                    if (ch == '(')
                    {
                        var parms = TextTools.TextBetween(w, i, ref l, '(', ')', out int? ax, out int? bx, withDelimiters: true);
                        if (parms != null)
                        {
                            if (parms != "()")
                            {
                                marker.MethodParams = new List<string>(TextTools.Split(parms.Substring(1, parms.Length - 2), ",", trimResults: true));
                            }

                            marker.MethodParamsString = parms;

                            if (bx != null) i = (int)bx;
                            hasParams = true;
                            retVal++;
                        }
                    }
                    else if (marker.Name == "this" && ch == '[')
                    {
                        var parms = TextTools.TextBetween(w, i, ref l, '[', ']', out int? ax, out int? bx, withDelimiters: true);
                        if (parms != null)
                        {
                            marker.MethodParamsString = parms;

                            marker.Kind = MarkerKind.This;
                            if (bx != null) i = (int)bx;
                        }
                    }
                    else if (ch == '<')
                    {
                        var parms = TextTools.TextBetween(w, i, ref l, '<', '>', out int? ax, out int? bx, withDelimiters: true);
                        if (parms != null)
                        {
                            marker.Generics = parms;
                            if (bx != null) i = (int)bx;
                        }
                    }
                    else if (!hasParams && i < c - 2 && w[i] == '=' && w[i + 1] == '>')
                    {
                        marker.Kind = MarkerKind.Property;
                        if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
                        return retVal;
                    }
                    else if (hasParams && i < c - 2 && w[i] == '=' && w[i + 1] == '>')
                    {
                        marker.Kind = MarkerKind.Method;
                        if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
                        return retVal;
                    }
                    else if (ch == ';' || ch == '=')
                    {
                        if (marker.Kind == MarkerKind.Code)
                        {
                            if (!hasParams)
                            {
                                marker.Kind = MarkerKind.Field;
                            }
                            else
                            {
                                marker.Kind = MarkerKind.Method;
                            }
                            if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
                            return retVal;
                        }
                    }
                    else if (ch == ':')
                    {
                        break;
                    }
                    else if (char.IsLetter(ch))
                    {
                        if (x == -1) x = i;
                        tsb.Append(ch);
                    }
                    else if (!AllowedChar(ch, true))
                    {
                        if (tsb.ToString() == "where")
                        {
                            i = x;
                            break;
                        }
                        else
                        {
                            x = -1;
                            tsb.Clear();
                        }
                    }
                }
            }

            if (i < c)
            {
                str = str.Substring(i).Trim();
                w = str.ToCharArray();
                c = str.Length;
                char lastNonspace = ' ';
                x = -1;
                List<string> ihits = new List<string>();
                if (str[0] == ':' || str[0] == 'w')
                {
                    tsb.Clear();

                    for (i = 0; i < c; i++)
                    {
                        ch = w[i];

                        if (ch == '(')
                        {
                            var parms = TextTools.TextBetween(w, i, ref l, '(', ')', out int? ax, out int? bx, withDelimiters: true);
                            if (parms != null)
                            {
                                tsb.Append(parms);
                                if (bx != null) i = (int)bx;
                            }
                        }
                        else if (ch == '<')
                        {
                            var parms = TextTools.TextBetween(w, i, ref l, '<', '>', out int? ax, out int? bx, withDelimiters: true);
                            if (parms != null)
                            {
                                tsb.Append(parms);
                                if (bx != null) i = (int)bx;
                            }
                        }
                        else if (ch == ',')
                        {
                            ihits.Add(tsb.ToString().Trim());
                            tsb.Clear();
                        }
                        else if (AllowedChar(ch, true))
                        {
                            tsb.Append(ch);
                        }
                        else if (!AllowedChar(ch, true))
                        {
                            // there's this crazy edge case where you can have a class named 'where'
                            if (tsb.ToString() == "where" && lastNonspace != ',' && lastNonspace != ':')
                            {
                                x = i - 5;
                                tsb.Clear();
                                break;
                            }
                            else
                            {
                                x = -1;
                                if (tsb.Length > 0)
                                    ihits.Add(tsb.ToString().Trim());
                                tsb.Clear();
                            }
                        }

                        if (!char.IsWhiteSpace(ch)) lastNonspace = ch;
                    }

                    if (tsb.Length > 0)
                    {
                        ihits.Add(tsb.ToString().Trim());
                        tsb.Clear();
                    }
                }

                if (x != -1)
                {
                    var sp = str.Substring(x);
                    marker.WhereClause = " " + sp;
                }

                if (ihits.Count > 0)
                {
                    tsb.Clear();
                    foreach (var s in ihits)
                    {
                        if (tsb.Length > 0) tsb.Append(", ");
                        tsb.Append(s);
                    }
                    marker.Inheritances = ihits;
                    marker.InheritanceString = " : " + tsb.ToString();
                }
            }

            if (marker.Inheritances != null)
            {
                if (marker.UnknownWords == null)
                {
                    marker.UnknownWords = new List<string>();
                }
                foreach (var s in marker.Inheritances)
                {
                    if (!string.IsNullOrEmpty(s) && !marker.UnknownWords.Contains(s))
                    {
                        marker.UnknownWords.Add(s);
                    }
                }
            }

            if (marker.Kind == MarkerKind.Code)
            {
                if (retVal == 2)
                {
                    marker.Kind = MarkerKind.Method;
                }
                else
                {
                    marker.Kind = MarkerKind.Property;
                }

                if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
            }

            return retVal;
        }

        private List<string> ParseGenerics(char[] chars, int startIndx)
        {
            int i = 0;
            List<string> retval = new List<string>();
            var generics = TextTools.TextBetween(chars, startIndx, ref i, '<', '>', out int? startIdx, out int? stopIdx);

            if (generics == null) return retval;

            var sp = TextTools.Split(generics, ",");
            var sb = new StringBuilder();

            foreach (var s in sp)
            {
                var g = s.Trim();

                if (g[0] == '<')
                {
                    retval.AddRange(ParseGenerics(chars, (int)startIdx));
                }
                else
                {
                    foreach (var ch in g)
                    {
                        if (AllowedChar(ch, true))
                        {
                            sb.Append(ch);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (sb.Length > 0) retval.Add(sb.ToString());
                }
            }

            return retval;
        }

        /// <summary>
        /// Check if this is an allowed identifier character.
        /// </summary>
        /// <param name="ch">Character to test.</param>
        /// <param name="alsoDot">Dot is valid.</param>
        /// <param name="first">Valid for first character.</param>
        /// <param name="types">Parsing for types includes the * pointer allowable.</param>
        /// <returns></returns>
        private bool AllowedChar(char ch, bool alsoDot, bool first = false, bool types = false)
        {
            if (first)
            {
                return char.IsLetter(ch) || ch == '@' || ch == '_';
            }
            else
            {
                return char.IsLetterOrDigit(ch) || ch == '@' || ch == '_' || (alsoDot && ch == '.') || (types && ch == '*');
            }
        }

        /// <summary>
        /// Perform post-scan cleanup tasks.
        /// </summary>
        /// <param name="markers"></param>
        private void PostScanTasks(TList markers)
        {
            int c = markers.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                if (markers[i].Children != null) PostScanTasks(markers[i].Children);

                if (i < c - 1)
                {
                    if (markers[i].Kind == MarkerKind.Do && markers[i + 1].Kind == MarkerKind.DoWhile)
                    {
                        markers[i].EndPos = markers[i + 1].EndPos;
                        markers[i].EndLine = markers[i + 1].EndLine;
                        markers[i].EndColumn = markers[i + 1].EndColumn;
                        markers[i].Content += markers[i + 1].Content;
                        markers[i].ScanHit += markers[i + 1].ScanHit;
                        markers[i].Kind = MarkerKind.DoWhile;

                        if (markers[i].Children == null && markers[i + 1].Children != null)
                        {
                            markers[i].Children = markers[i + 1].Children;
                        }
                        else if (markers[i].Children != null && markers[i + 1].Children != null)
                        {
                            if (markers[i].Children is List<TMarker> l)
                            {
                                l.AddRange(markers[i + 1].Children);
                            }
                            else
                            {
                                foreach (var m in markers[i + 1].Children)
                                {
                                    markers[i].Children.Add(m);
                                }
                            }
                        }

                        PostScanTasks(markers[i].Children);
                        markers.RemoveAt(i + 1);
                        c--;
                    }
                }
            }
        }

        public void StripAccessModifiers(string sample, out AccessModifiers modifiers, out string leftOver)
        {
            sample = sample.Replace("\r", "").Replace("\n", "").Trim();

            var sret = new List<string>();
            var m = AccessModifiers.None;
            var sb = new StringBuilder();

            var words = TextTools.Words(sample);

            if (words == null || words.Length == 0)
            {
                leftOver = "";
                modifiers = AccessModifiers.None;
                return;
            }

            foreach (var word in words)
            {
                var amtest = TextTools.TitleCase(word);

                if (Enum.TryParse<AccessModifiers>(amtest, out var result))
                {
                    m |= result;
                }
                else
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(word);
                }
            }

            modifiers = m;
            leftOver = sb.ToString();
        }

        private int CountLines(string text)
        {
            int count = 0;
            foreach (var ch in text)
            {
                if (ch == '\n') count++;
                else if (ch == '\r') continue;
                else if (!char.IsWhiteSpace(ch)) return count;
            }

            return count;
        }

        /// <summary>
        /// Calculate the line column from the present position.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ColumnFromHere(char[] chars, int pos)
        {
            int c = 1;
            int i;

            for (i = pos - 1; i >= 0; i--)
            {
                var ch = chars[i];
                if (ch == '\n') return c;

                c++;
            }

            return 1;
        }

        private bool ContainsCode(MarkerKind kind)
        {
            return kind == MarkerKind.Method || kind == MarkerKind.Constructor || kind == MarkerKind.Destructor || kind == MarkerKind.Get || kind == MarkerKind.Set || kind == MarkerKind.Add || kind == MarkerKind.Remove || kind == MarkerKind.Operator;
        }

        #endregion CSharp Code Parsing Internals
    }
}