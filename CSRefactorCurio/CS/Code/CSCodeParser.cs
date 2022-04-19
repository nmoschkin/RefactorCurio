using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using DataTools.SortedLists;
using DataTools.Text;

using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.LocalLogger;
using Microsoft.VisualStudio.RemoteSettings;
using Microsoft.VisualStudio.Shell.Interop;

namespace DataTools.CSTools
{
    /// <summary>
    /// Code Parser Output File
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/></typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
    public class OutputFile<TElem, TList> where TElem: IMarker, new() where TList : IMarkerList<TElem>, new()
    {
        /// <summary>
        /// The text of the new file.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The name of the saved file.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <returns></returns>
        public bool Write()
        {
            try
            {
                var p = Path.GetDirectoryName(Filename);
                Directory.CreateDirectory(p);

                File.WriteAllText(Filename, Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create a new file from the given parameters.
        /// </summary>
        /// <typeparam name="TI">The type of <see cref="IMarker"/></typeparam>
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
        /// <param name="path">The path of the new file.</param>
        /// <param name="file">The <see cref="RenderedFile{TElem, TList}"/> information.</param>
        /// <param name="lines">The original file split into lines.</param>
        /// <param name="sepDirs">True to put different kinds of items in separate directories.</param>
        /// <param name="parser">The code parser instance.</param>
        /// <returns>A new output file.</returns>
        public static OutputFile<TI, TL> NewFile<TI, TL>(string path, RenderedFile<TI, TL> file, string[] lines, bool sepDirs, CSCodeParser<TI, TL> parser = null) where TI: IMarker<TI, TL>, new() where TL : IMarkerList<TI>, new()
        {
            return NewFile<TI, TL>(path, file.Markers[0].Kind, file.Markers[0].Name, FormatOutputText<TI, TL>(file.Markers, lines, file.PreambleEnd, file.PreambleBegin), sepDirs, parser);
        }

        /// <summary>
        /// Create a new file from the given parameters.
        /// </summary>
        /// <typeparam name="TI">The type of <see cref="IMarker"/></typeparam>
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
        /// <param name="path">The path of the new file.</param>
        /// <param name="mkind">Specifies the top-level marker kind (for determining the destination folder)</param>
        /// <param name="name">The name of the new file (without extension)</param>
        /// <param name="text">The contents of the new file</param>
        /// <param name="sepDirs">True to put different kinds of items in separate directories.</param>
        /// <param name="cs">An optional reference <see cref="CSCodeParser{TElem, TList}"/> with configuration information.</param>
        /// <returns>A new output file.</returns>
        public static OutputFile<TI, TL> NewFile<TI, TL>(string path, MarkerKind mkind, string name, string text, bool sepDirs, CSCodeParser<TI, TL> cs = null) where TI : IMarker<TI, TL>, new() where TL : IMarkerList<TI>, new()
        {
            path = path.Trim().Trim('\\');
            string kind = "";
            switch (mkind)
            {
                case MarkerKind.Class:
                    kind = cs?.ClassDirName ?? "Classes";
                    break;

                case MarkerKind.Interface:
                    kind = cs?.InterfaceDirName ?? "Interfaces";
                    break;

                case MarkerKind.Struct:
                    kind = cs?.StructDirName ?? "Structs";
                    break;

                case MarkerKind.Enum:
                    kind = cs?.EnumDirName ?? "Enums";
                    break;

            }
            if (sepDirs && !string.IsNullOrEmpty(kind))
            {
                return new OutputFile<TI, TL>
                {
                    Text = text,
                    Filename = $"{path}\\{kind}\\{name}.cs"
                };
            }
            else
            {
                return new OutputFile<TI, TL>
                {
                    Text = text,
                    Filename = $"{path}\\{name}.cs"
                };

            }
        }

        /// <summary>
        /// Create a simple <see cref="OutputFile{TElem, TList}"/> instance with just the specified filename and text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static OutputFile<TElem, TList> NewFile(string filename, string text)
        {
            return new OutputFile<TElem, TList>
            {
                Text = text,
                Filename = filename
            };
        }

        /// <summary>
        /// Format the specified markers into a text for saving to a new class file.
        /// </summary>
        /// <typeparam name="TI">The type of <see cref="IMarker"/></typeparam>
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
        /// <param name="markers">The markers to format.</param>
        /// <param name="lines">The lines of the source file.</param>
        /// <param name="preambleTo">The end position of the common preamble calculated from the source file.</param>
        /// <param name="preambleFrom">Optional start position of the common preamble.</param>
        /// <returns>Formatted Code</returns>
        public static string FormatOutputText<TI, TL>(TL markers, string[] lines, int preambleTo = -1, int preambleFrom = 0) where TI : IMarker, new() where TL : IList<TI>, new()
        {

            string t = "";
            string lastns = null;
            string textOut = "";

            var pre = GetPreamble(lines, preambleTo, preambleFrom);

            if (pre != null) textOut += pre + "\r\n";

            int x = 0, c = markers.Count;

            foreach (var marker in markers)
            {
                t = "";

                for (var i = marker.StartLine; i <= marker.EndLine; i++)
                {
                    if (t != "") t += "\r\n";
                    t += lines[i];
                }

                if (marker.Namespace != null)
                {
                    if (lastns != marker.Namespace)
                    {
                        if (lastns != null)
                        {
                            textOut += "\r\n}\r\n\r\n";
                        }

                        textOut += $"namespace {marker.Namespace}\r\n{{\r\n";
                        lastns = marker.Namespace;
                    }
                }

                textOut += t;
                if (x != c - 1) textOut += "\r\n\r\n";

                if (marker.Namespace != null)
                {
                    if (x == c - 1 || markers[x + 1].Namespace != lastns)
                        textOut += "\r\n}\r\n";
                }
                x++;
            }


            return textOut;
        }

        /// <summary>
        /// Gets the preamble lines.
        /// </summary>
        /// <param name="lines">The source lines.</param>
        /// <param name="preambleTo">End position.</param>
        /// <param name="preambleFrom">Start Position.</param>
        /// <returns></returns>
        public static string GetPreamble(string[] lines, int preambleTo, int preambleFrom)
        {
            string s = "";

            for (var i = preambleFrom; i <= preambleTo; i++)
            {
                if (s != "") s += "\r\n";
                s += lines[i];
            }

            return s;
        }



    }

    /// <summary>
    /// C# Code Parser Class
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/></typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
    public class CSCodeParser<TElem, TList> where TElem : IMarker<TElem, TList>, new() where TList : IMarkerList<TElem>, new()
    {


        protected string[] lines = null;
        protected string text = null;

        protected object SyncRoot { get; } = new object();

        protected int preambleTo = 0;

        protected TList markers = new TList();
        protected RenderedFile<TElem, TList> mfile = null;

        protected List<string> lastErrors = new List<string>();

        protected string filename = null;

        protected string[] outputFiles = new string[0];

        /// <summary>
        /// Gets or sets the output path for files generated from this parser.
        /// </summary>
        public virtual string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the list of output files generated from this parser.
        /// </summary>
        public virtual string[] OutputFiles => outputFiles;

        /// <summary>
        /// Gets or sets the 'Interfaces' directory name (default is 'Contracts')
        /// </summary>
        public virtual string InterfaceDirName { get; set; } = "Contracts";

        /// <summary>
        /// Gets or sets the 'Classes' directory name (default is none)
        /// </summary>
        public virtual string ClassDirName { get; set; } = "";

        /// <summary>
        /// Gets or stets the 'Enums' directory name (default is 'Enums')
        /// </summary>
        public virtual string EnumDirName { get; set; } = "Enums";

        /// <summary>
        /// Gets or stets the 'Structs' directory name (default is 'Structs')
        /// </summary>
        public virtual string StructDirName { get; set; } = "Structs";

        /// <summary>
        /// Gets or sets a value indicating that files containing different types of objects will go in different subdirectories beneath the selected output directory.
        /// </summary>
        public virtual bool SeparateDirs { get; set; } = true;

        /// <summary>
        /// Gets a value indicating if the last parse was successful.
        /// </summary>
        public virtual bool ParseSuccess { get; protected set; } = false;

        /// <summary>
        /// Gets a value indicating that the parser loaded the file lazily and has not yet parsed it.
        /// </summary>
        public virtual bool IsLazyLoad { get; protected set; } = false; 

        /// <summary>
        /// Gets the name of the original source file.
        /// </summary>
        public virtual string Filename
        {
            get => filename;
            protected set => filename = value;
        }

        /// <summary>
        /// Gets a list of the last errors.
        /// </summary>
        public virtual IReadOnlyList<string> Errors
        {
            get => lastErrors;
        }

        /// <summary>
        /// Load and optionally parse a C# code file.
        /// </summary>
        /// <param name="filename">The valid relative or absolute pathname to the source file.</param>
        /// <param name="lazy">True to load the file without parsing it.</param>
        /// <exception cref="FileNotFoundException"></exception>
        public virtual void LoadFile(string filename, bool lazy = false)
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
        /// Reread the file from the disk and reparse its contents.
        /// </summary>
        /// <remarks>
        /// This method will unset <see cref="IsLazyLoad"/>.
        /// </remarks>
        public void Refresh()
        {
            lock (SyncRoot)
            {
                IsLazyLoad = false;
                Parse(File.ReadAllText(Filename));
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
        /// <returns>A new <see cref="CSCodeParser{TElem, TList}"/> or derived object.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static CSCodeParser<TElem, TList> LoadFromFile(string filename, bool lazy = false)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            var res = new CSCodeParser<TElem, TList>();
            res.LoadFile(filename, lazy);
            return res;
        }

        /// <summary>
        /// Write the results of the refactor and reorganized file structure to disk.
        /// </summary>
        /// <param name="path">The destination path.</param>
        /// <param name="separateDirs">True to use separate directories for different item types.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public virtual bool OutputMarkers(string path = null, bool? separateDirs = null)
        {
            if (separateDirs is bool bo)
            {
                SeparateDirs = bo;
            }

            if (path != null)
            {
                OutputPath = path;
            }
            else
            {
                path = OutputPath;
            }

            if (!Directory.Exists(OutputPath)) throw new DirectoryNotFoundException(OutputPath);

            TList seen = new TList();
            
            if (mfile == null) return false;
            if (markers == null) return false;

            foreach (var marker in mfile.Markers)
            {
                if (seen.Contains(marker)) continue;

                var mlist = new TList();

                var name = marker.Name;
                var ns = marker.Namespace;
                var kind = marker.Kind;

                foreach (var m in markers)
                {
                    if (seen.Contains(m) || mlist.Contains(m)) continue;

                    if (m.Name == name && m.Namespace == ns && m.Kind == kind)
                    {
                        mlist.Add(m);
                    }
                }

                QuickSort.Sort(mlist, (a, b) =>
                {
                    int x = string.Compare(a.Namespace, b.Namespace);

                    if (x == 0)
                    {
                        x = string.Compare(a.Name, b.Name);
                        if (x == 0)
                        {
                            x = string.Compare(a.Generics, b.Generics);
                        }
                    }

                    return x;
                });

                if (((IList<TElem>)mlist).Count == 0) continue;
                if (seen is List<TElem> l)
                {
                    l.AddRange(mlist);
                }
                else
                {
                    foreach (var m in mlist)
                    {
                        seen.Add(m);
                    }
                }

                var mf = new RenderedFile<TElem, TList>()
                {
                    Lines = mfile.Lines,
                    Markers = mlist,
                    PreambleBegin = mfile.PreambleBegin,
                    PreambleEnd = mfile.PreambleEnd
                };

                var file = OutputFile<TElem, TList>.NewFile(path, mf, lines, SeparateDirs, this);
                file.Write();
            }

            return true;
        }

        /// <summary>
        /// Gets the current list of markers that were parsed from source text.
        /// </summary>
        public virtual TList Markers => markers;

        /// <summary>
        /// Gets the end position of the preamble in the source text.
        /// </summary>
        public virtual int PreambleTo => preambleTo;

        /// <summary>
        /// Gets the source code text.
        /// </summary>
        public virtual string Text => text;

        /// <summary>
        /// Gets the source code text broken into lines.
        /// </summary>
        public virtual string[] Lines => lines;

        public override string ToString()
        {
            return Filename;
        }

        /// <summary>
        /// Parse the given C# source code text.
        /// </summary>
        /// <param name="text">The C# source code text to parse.</param>
        /// <returns>True if successful.</returns>
        protected virtual bool Parse(string text)
        {
            lock (SyncRoot)
            {
                ParseSuccess = false;

                this.text = text;
                this.lines = text.Replace("\r\n", "\n").Split('\n');

                try
                {
                    markers = InternalRawParseCSCode(text.ToCharArray());

                    var cf = new RenderedFile<TElem, TList>()
                    {
                        PreambleBegin = 0,
                        PreambleEnd = preambleTo,
                        Markers = markers ?? new TList()
                    };

                    mfile = cf;

                    foreach (var marker in markers)
                    {
                        SetParentFile(marker, cf);
                    }

                    ParseSuccess = true;
                    IsLazyLoad = false;
                }
                catch (SyntaxErrorException ex)
                {
                    lastErrors.Add(ex.Message);
                    IsLazyLoad = true;
                }

                return ParseSuccess;
            }
        }

        /// <summary>
        /// Set the parent <see cref="RenderedFile{TElem, TList}"/> object to the specified marker and its descendents.
        /// </summary>
        /// <param name="marker">The marker to modify.</param>
        /// <param name="file">The parent file object.</param>
        protected void SetParentFile(TElem marker, RenderedFile<TElem, TList> file)
        {
            marker.ParentFile = file;

            if (marker.Children != null)
            {
                foreach (var child in marker.Children)
                {
                    SetParentFile(child, file);
                }
            }
        }

        #region CSharp Code Parsing Internals

        protected static Dictionary<MarkerKind, Regex> patterns = new Dictionary<MarkerKind, Regex>();
        protected static Regex genericPatt;
        static CSCodeParser()
        {
            patterns.Add(MarkerKind.Using, new Regex(@"using (.+)\s*;"));
            patterns.Add(MarkerKind.Namespace, new Regex(@"namespace (.+)"));
            patterns.Add(MarkerKind.This, new Regex(@".*(this)\s*\[.+\].*"));
            patterns.Add(MarkerKind.Class, new Regex(@".*class\s+([A-Za-z0-9_@.]+).*"));
            patterns.Add(MarkerKind.Interface, new Regex(@".*interface\s+([A-Za-z0-9_@.]+).*"));
            patterns.Add(MarkerKind.Struct, new Regex(@".*struct\s+([A-Za-z0-9_@.]+).*"));
            patterns.Add(MarkerKind.Enum, new Regex(@".*enum\s+([A-Za-z0-9_@.]+).*"));
            patterns.Add(MarkerKind.Record, new Regex(@".*record\s+([A-Za-z0-9_@.]+).*"));
            patterns.Add(MarkerKind.Delegate, new Regex(@".*delegate\s+.+\s+([A-Za-z0-9_@.]+)\(.*\)\s*;"));
            patterns.Add(MarkerKind.Event, new Regex(@".*event\s+.+\s+([A-Za-z0-9_@.]+)\s*"));
            patterns.Add(MarkerKind.Const, new Regex(@".*const\s+.+\s+([A-Za-z0-9_@.]+)\s*"));
            patterns.Add(MarkerKind.Operator, new Regex(@".*operator\s+(\S+)\(.*\)"));
            patterns.Add(MarkerKind.ForLoop, new Regex(@"\s*for\s*\(.*;.*;.*\)"));
            patterns.Add(MarkerKind.DoWhile, new Regex(@"\s*while\s*\(.*\)\s*;"));
            patterns.Add(MarkerKind.While, new Regex(@"\s*while\s*\(.*\)"));
            patterns.Add(MarkerKind.Switch, new Regex(@"\s*switch\s*\(.+\)"));
            patterns.Add(MarkerKind.Case, new Regex(@"\s*case\s*\(.+\)\s*:"));
            patterns.Add(MarkerKind.UsingBlock, new Regex(@"\s*using\s*\(.*\)"));
            patterns.Add(MarkerKind.Lock, new Regex(@"\s*lock\s*\(.*\)"));
            patterns.Add(MarkerKind.Unsafe, new Regex(@"\s*unsafe\s*$"));
            patterns.Add(MarkerKind.Fixed, new Regex(@"\s*fixed\s*"));
            patterns.Add(MarkerKind.ForEach, new Regex(@"\s*foreach\s*\(.*\)"));
            patterns.Add(MarkerKind.Do, new Regex(@"\s*do\s*(\(.+\)|$)"));
            patterns.Add(MarkerKind.Else, new Regex(@"\s*else\s*.*"));
            patterns.Add(MarkerKind.ElseIf, new Regex(@"\s*else if\s*(\(.+\)|$)"));
            patterns.Add(MarkerKind.If, new Regex(@"\s*if\s*(\(.+\)|$)"));
            patterns.Add(MarkerKind.Get, new Regex(@"\s*get\s*($|\=\>).*"));
            patterns.Add(MarkerKind.Set, new Regex(@"\s*set\s*($|\=\>).*"));
            patterns.Add(MarkerKind.Add, new Regex(@"\s*add\s*($|\=\>).*"));
            patterns.Add(MarkerKind.Remove, new Regex(@"\s*remove\s*($|\=\>).*"));
            patterns.Add(MarkerKind.FieldValue, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*\=.+;$"));
            patterns.Add(MarkerKind.Method, new Regex(@".* ([A-Za-z0-9_@.]+).*\s*\(.*\)\s*(;|\=\>|$|\s*where\s*.+:.+)"));
            patterns.Add(MarkerKind.EnumValue, new Regex(@"\s*([A-Za-z0-9_@.]+)(\s*=\s*(.+))?[,]?"));
            patterns.Add(MarkerKind.Property, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*($|\=\>).*"));
            patterns.Add(MarkerKind.Field, new Regex(@".+\s+([A-Za-z0-9_@.]+)\s*;$"));

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
        protected virtual TList InternalRawParseCSCode(char[] chars)
        {

            lock (SyncRoot)
            {
                int i, j, c = chars.Length;

                TList markers = new TList();
                TElem currMarker = default;
                var strack = new Stack<MarkerKind>();
                var stack = new Stack<TElem>();
                var listStack = new Stack<TList>();

                StringBuilder cw = new StringBuilder();
                StringBuilder ms = new StringBuilder();
                StringBuilder sb;
                
                ms.Capacity = chars.Length;

                int startPos = 0;
                int scanStartPos = 0;

                int startLine = 0;

                int currLine = 1;

                int currLevel = 0;

                string currNS = "";

                int pre = -1;

                bool clo = false;

                Regex currCons = null;
                Regex currDecons = null;

                string currName = null;
                MarkerKind currPatt = MarkerKind.Code;

                List<string> attrs = null;

                for (i = 0; i < c; i++)
                {
                    if (chars[i] == '\r' || chars[i] == '\n')
                    {
                        ms.Append(" ");
                    }
                    else
                    {
                        ms.Append(chars[i]);
                    }

                    if (chars[i] == '\n')
                    {
                        currLine++;
                    }
                    else if (chars[i] == '\'')
                    {
                        TextTools.QuoteFromHere(chars, i, ref currLine, out int? spt, out int? ept, quoteChar: '\'', withQuotes: true);
                        i = (int)ept;
                    }
                    else if (chars[i] == '\"')
                    {
                        TextTools.QuoteFromHere(chars, i, ref currLine, out int? spt, out int? ept, withQuotes: true);
                        i = (int)ept;
                    }
                    else if (chars[i] == '(')
                    {
                        clo = true;
                    }
                    else if (chars[i] == '[' && !clo)
                    {
                        ms.Remove(ms.Length - 1, 1);

                        var sl = currLine;
                        var lookahead = TextTools.TextBetween(chars, i, ref currLine, '[', ']', out int? spt, out int? ept, withDelimiters: true);
                        
                        if (lookahead == null) continue;
                        
                        if (!Regex.IsMatch(lookahead, @"\[\s*[\w\d._@]+\s*\(?.*?\)?\]"))
                        {
                            i = (int)ept;
                            continue;
                        }

                        if (attrs == null) attrs = new List<string>();
                        attrs.Add(lookahead);

                        i = (int)ept;
                        scanStartPos = i + 1;
                    }
                    else if (chars[i] == ';' || (chars[i] == ',' && currPatt == MarkerKind.Enum))
                    {
                        clo = false;

                        if ((currPatt == MarkerKind.Enum) || ((currPatt & MarkerKind.IsBlockLevel) != MarkerKind.IsBlockLevel))
                        {
                            var lookback = TextTools.OneSpace(ms.ToString()).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos + 1).Replace("\r", "").Replace("\n", "").Trim());
                            Match testEnum = null;
                            
                            if (currPatt == MarkerKind.Enum)
                            {
                                testEnum = patterns[MarkerKind.EnumValue].Match(lookback);

                            }
                            
                            if (testEnum != null && testEnum.Success)
                            {
                                currMarker = new TElem
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
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
                                currMarker = new TElem
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    EndPos = i,
                                    EndLine = currLine,
                                    EndColumn = ColumnFromHere(chars, i),
                                    Level = currLevel,
                                    ScanHit = lookback,
                                    Attributes = attrs
                                };

                                TypeAndMethodParse(lookback, chars, scanStartPos, i, currMarker);
                            }

                            markers.Add(currMarker);
                        }

                        scanStartPos = startPos = i + 1;
                        ms.Clear();
                        if (i < c - 1 && chars[i + 1] == '\n')
                        {
                            startLine = currLine + 1;
                        }
                        else
                        {
                            startLine = currLine;
                        }
                        attrs = null;
                    }
                    else if (chars[i] == '{')
                    {
                        ms.Remove(ms.Length - 1, 1);
                        clo = false;
                        ++currLevel;

                        strack.Push(currPatt);

                        if ((currPatt == MarkerKind.Event) || currPatt == MarkerKind.Property || ((currPatt & MarkerKind.IsBlockLevel) != MarkerKind.IsBlockLevel))
                        {
                            var lookback = TextTools.OneSpace(ms.ToString()).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos).Replace("\r", "").Replace("\n", "").Trim());
                            Match cons = currCons?.Match(lookback) ?? null;
                            Match ops = patterns[MarkerKind.Operator].Match(lookback);
                            var lb = RemoveWhere(lookback);

                            if (cons != null && cons.Success && !ops.Success)
                            {
                                currMarker = new TElem
                                {
                                    StartPos = startPos,
                                    Namespace = currNS,
                                    StartLine = startLine,
                                    StartColumn = ColumnFromHere(chars, startPos),
                                    Kind = MarkerKind.Constructor,
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
                                    currMarker = new TElem
                                    {
                                        Namespace = currNS,
                                        StartPos = startPos,
                                        StartLine = startLine,
                                        StartColumn = ColumnFromHere(chars, startPos),
                                        Kind = MarkerKind.Destructor,
                                        MethodParamsString = "()",
                                        Name = currName,
                                        Level = currLevel,
                                        ScanHit = lookback,
                                        Attributes = attrs
                                    };

                                    currPatt = MarkerKind.Destructor;
                                    markers.Add(currMarker);
                                }
                                else
                                {

                                    currMarker = new TElem
                                    {
                                        Namespace = currNS,
                                        StartPos = startPos,
                                        StartLine = startLine,
                                        StartColumn = ColumnFromHere(chars, startPos),
                                        Level = currLevel,
                                        ScanHit = lookback,
                                        Attributes = attrs
                                    };

                                    TypeAndMethodParse(lookback, chars, scanStartPos, i, currMarker);

                                    currPatt = currMarker.Kind;

                                    if (currMarker.Kind == MarkerKind.Namespace)
                                    {
                                        currNS = currMarker.Name;
                                        currMarker.Namespace = currNS;
                                        if (pre == -1)
                                        {
                                            pre = startPos - 1;
                                        }
                                    }

                                    markers.Add(currMarker);

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

                        scanStartPos = startPos = i + 1;
                        ms.Clear();

                        if (i < c - 1 && chars[i + 1] == '\n')
                        {
                            startLine = currLine + 1;
                        }
                        else
                        {
                            startLine = currLine;
                        }

                        attrs = null;
                    }
                    else if (chars[i] == '}')
                    {
                        clo = false;
                        if (currPatt == MarkerKind.Enum)
                        {
                            var lookback = TextTools.OneSpace(ms.ToString()).Trim(); // TextTools.OneSpace(new string(chars, scanStartPos, i - scanStartPos).Replace("\r", "").Replace("\n", "").Trim());
                            var testEnum = patterns[MarkerKind.EnumValue].Match(lookback);

                            if (testEnum.Success)
                            {
                                currMarker = new TElem
                                {
                                    Namespace = currNS,
                                    StartPos = startPos,
                                    StartLine = startLine,
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


                        --currLevel;
                        currPatt = strack.Pop();

                        currMarker = stack.Pop();

                        if (currMarker != null)
                        {
                            currMarker.EndPos = i;
                            currMarker.EndLine = currLine;
                            currMarker.EndColumn = ColumnFromHere(chars, i);
                        }

                        currMarker.Children = markers;
                        markers = listStack.Pop();

                        scanStartPos = startPos = i + 1;

                        ms.Clear();

                        if (i < c - 1 && chars[i + 1] == '\n')
                        {
                            startLine = currLine + 1;
                        }
                        else
                        {
                            startLine = currLine;
                        }

                    }
                    else if ((i < c - 1) && ((chars[i] == '/' && chars[i + 1] == '/') || (chars[i] == '#')))
                    {
                        ms.Remove(ms.Length - 1, 1);
                        ms.Append(' ');
                        clo = false;
                        currMarker = new TElem()
                        {
                            StartColumn = ColumnFromHere(chars, i),
                            StartLine = currLine,
                            StartPos = i,
                            Level = currLevel,
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
                                currMarker.EndColumn = ColumnFromHere(chars, j - 1);
                                currMarker.EndLine = currLine;
                                currMarker.EndPos = j - 1;
                                currMarker.Content = sb.ToString();
                                currMarker.Kind = docs ? MarkerKind.XMLDoc : MarkerKind.LineComment;

                                markers.Add(currMarker);

                                currLine++;

                                if (docs)
                                {
                                    startPos = scanStartPos = j + 1;
                                    startLine = currLine;
                                }
                                break;
                            }

                        }

                        if (j >= c) break;
                        i = j;
                    }
                    else if ((i < c - 3) && (chars[i] == '/' && chars[i + 1] == '*'))
                    {
                        ms.Remove(ms.Length - 1, 1);
                        ms.Append(' ');

                        clo = false;
                        currMarker = new TElem()
                        {
                            StartColumn = ColumnFromHere(chars, i),
                            StartLine = currLine,
                            StartPos = i,
                            Level = currLevel,
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

                                scanStartPos = j + 2;
                                if (i < c - 2 && chars[i + 2] == '\n')
                                {
                                    startLine = currLine + 1;
                                }
                                else
                                {
                                    startLine = currLine;
                                }
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

        /// <summary>
        /// A list of filtered keywords for the C# language.
        /// </summary>
        private static readonly string[] deletes = new string[] { "operator", "explicit", "implicit", "class", "interface", "record", "struct", "namespace", "public", "private", "static", "async", "abstract", "explicit", "implicit", "const", "readonly", "unsafe", "fixed", "delegate", "event", "virtual", "protected", "internal", "override", "new", "using", "get", "set", "add", "remove", "enum" };

        /// <summary>
        /// Parse the type, name and method parameters from a lookback string.
        /// </summary>
        /// <param name="lookback">The string to scan.</param>
        /// <param name="marker">The destination marker.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="SyntaxErrorException"></exception>
        private int TypeAndMethodParse(string lookback, char[] chars, int start, int stop, TElem marker)
        {
            int l = 0, i;
            var str = lookback;
            if (lookback[0] == '=') return 0;
            if (marker.Kind == MarkerKind.EnumValue) return 0;

            IList<string> dels = deletes;
            List<string> fd = new List<string>();
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

                if (i == 0 && !AllowedName(ch, false, true)) throw new SyntaxErrorException();

                if (AllowedName(ch, true))
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
                else if (ch == ' ')
                {
                    if (lww)
                    {
                        var del = tsb.ToString();
                        if (dels.Contains(del))
                        {
                            tsb.Clear();
                            lww = false;
                            switch(del)
                            {
                                case "public":
                                case "private":
                                case "internal":
                                case "protected":
                                    marker.AccessModifiers |= (AccessModifiers)Enum.Parse(typeof(AccessModifiers), TextTools.TitleCase(del));
                                    break;

                                case "class":
                                case "event":
                                case "interface":
                                case "struct":
                                case "record":
                                case "const":
                                case "get":
                                case "set":
                                case "add":
                                case "remove":
                                case "enum":
                                case "namespace":
                                case "using":
                                    marker.Kind = (MarkerKind)Enum.Parse(typeof (MarkerKind), TextTools.TitleCase(del));
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

                                case "virtual":
                                    marker.IsVirtual = true;
                                    break;

                                default:
                                    break;
                            }

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
                    switch(marker.Kind)
                    {
                        case MarkerKind.Class:
                        case MarkerKind.Struct:
                        case MarkerKind.Interface:
                        case MarkerKind.Record:
                        case MarkerKind.Enum:
                            marker.Name = tsb.ToString();
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
                return 0;
            }

            for (; i < c; i++)
            {
                ch = w[i];

                if (AllowedName(ch, true))
                {
                    //if (ch == '.') eii = true;
                    nsb.Append(ch);
                }
                else
                {
                    if (marker.DataType == "bool" && nsb.ToString() == "operator")
                    {
                        nsb.Clear();
                        i++;
                        while (i < c && w[i] != '(' && !char.IsWhiteSpace(w[i]))
                        {
                            nsb.Append(w[i]);
                            i++;
                        }
                        marker.Kind = MarkerKind.Operator;
                    }

                    break;
                }
            }

            if (nsb.Length > 0)
            {
                marker.Name = nsb.ToString();   
            }
            
            bool inh = false;

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
                    else if (ch == '<')
                    {
                        var parms = TextTools.TextBetween(w, i, ref l, '<', '>', out int? ax, out int? bx, withDelimiters: true);
                        if (parms != null)
                        {
                            marker.Generics = parms;
                            if (bx != null) i = (int)bx;
                        }
                    }
                    else if ((!hasParams) && (i < c - 2) && (w[i] == '=') && (w[i + 1] == '>'))
                    {
                        marker.Kind = MarkerKind.Property;
                        if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
                        return retVal;
                    }
                    else if ((hasParams) && (i < c - 2) && (w[i] == '=') && (w[i + 1] == '>'))
                    {
                        marker.Kind = MarkerKind.Method;
                        if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
                        return retVal;
                    }
                    else if (ch == ';' || ch == '=')
                    {
                        if (marker.Kind == MarkerKind.Code)
                        {
                            marker.Kind = MarkerKind.Field;
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
                    else if (!AllowedName(ch, true))
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
                char xz = ' ';
                x = -1;
                List<string> ihits = new List<string>();
                if (str[0] == ':')
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
                            inh = true;
                            ihits.Add(tsb.ToString().Trim());
                            tsb.Clear();
                        }
                        else if (AllowedName(ch, true))
                        {
                            tsb.Append(ch);
                        }
                        else if (!AllowedName(ch, true))
                        {
                            if (tsb.ToString() == "where" && xz != ',')
                            {
                                x = i - 5;
                                tsb.Clear();
                                break;
                            }
                            else 
                            {
                                x = -1;
                                inh = false;
                                if (tsb.Length > 0)
                                    ihits.Add(tsb.ToString().Trim());
                                tsb.Clear();
                            }
                        }

                        if (!char.IsWhiteSpace(ch)) xz = ch;
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
                    marker.WhereClause = sp;
                }
                
                if (ihits.Count > 0)
                {
                    tsb.Clear();
                    foreach(var s in ihits)
                    {
                        if (tsb.Length > 0) tsb.Append(", ");
                        tsb.Append(s);
                    }

                    marker.Inheritance = tsb.ToString();
                }
                

            }

            if (marker.Kind == MarkerKind.Code)
            {
                if (retVal == 2)
                {
                    marker.Kind = MarkerKind.Property;
                }
                else
                {
                    marker.Kind = MarkerKind.Method;
                }

                if (eii) marker.Kind |= MarkerKind.ExplicitImplementation;
            }

            return retVal;
        }

        /// <summary>
        /// Check if this is an allowed identifier charachter.
        /// </summary>
        /// <param name="ch">Character to test.</param>
        /// <param name="alsoDot">Dot is valid.</param>
        /// <param name="first">Valid for first character.</param>
        /// <returns></returns>
        private bool AllowedName(char ch, bool alsoDot, bool first = false)
        {
            if (first)
            {
                return char.IsLetter(ch) || ch == '@' || ch == '_';
            }
            else
            {
                return char.IsLetterOrDigit(ch) || ch == '@' || ch == '_' || (alsoDot && ch == '.');
            }
        }

        /// <summary>
        /// Remove the where clause from the specified string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A new string with its where clause stripped out.</returns>
        private string RemoveWhere(string value)
        {
            bool io = false;

            int c = value.Length;
            var sb = new StringBuilder();   

            for (int i = c - 1; i >= 0; i--)
            {
                char ch = value[i];

                if (ch == ')') io = true;
                else if (ch == '(') io = false;

                if (char.IsWhiteSpace(ch))
                {
                    sb.Clear();
                }
                else
                {
                    sb.Insert(0, ch);
                }

                if (sb.ToString() == "where")
                {
                    if (!io) value = value.Substring(0, i);
                    
                    sb.Clear();
                }
            }

            return value;
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
                            if (markers[i].Children is List<TElem> l)
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
                    //else if (markers[i].Kind == MarkerKind.XMLDoc || markers[i].Kind == MarkerKind.LineComment)
                    //{
                    //    int x = i;

                    //    while (i < c && (markers[i].Kind == MarkerKind.XMLDoc || markers[i].Kind == MarkerKind.LineComment))
                    //    {
                    //        i++;
                    //    }

                    //    if (i < c)
                    //    {
                    //        var mknew = new TElem();

                    //        mknew.StartPos = markers[x].StartPos;
                    //        mknew.StartLine = markers[x].StartLine;
                    //        mknew.StartColumn = markers[x].StartColumn;

                    //        mknew.Children = new TList();
                    //        mknew.Content = "";


                    //        for (int z = x; z <= i; z++)
                    //        {
                    //            if (markers[z].Children != null) PostScanTasks(markers[z].Children);
                    //            mknew.Content += markers[z].Content;
                    //            mknew.Children.Add(markers[z]);
                    //        }

                    //        mknew.EndPos = markers[i].EndPos;
                    //        mknew.EndLine = markers[i].EndLine;
                    //        mknew.EndColumn = markers[i].EndColumn;

                    //        mknew.Kind = MarkerKind.Consolidation;
                    //        mknew.Name = markers[i].Name;
                    //        mknew.ScanHit = markers[i].ScanHit;
                    //        mknew.Generics = markers[i].Generics;

                    //        mknew.AccessModifiers = markers[i].AccessModifiers;
                    //        mknew.IsAbstract = markers[i].IsAbstract;
                    //        mknew.IsVirtual = markers[i].IsVirtual;
                    //        mknew.IsStatic = markers[i].IsStatic;
                    //        mknew.IsExtern = markers[i].IsExtern;
                    //        mknew.IsOverride = markers[i].IsOverride;
                    //        mknew.IsNew = markers[i].IsNew;
                    //        mknew.IsAsync = markers[i].IsAsync;

                    //        if (markers is List<TElem> l)
                    //        {
                    //            l.RemoveRange(x, (i - x) + 1);
                    //        }
                    //        else
                    //        {
                    //            for (int y = 0; y < (i - x) + 1; y++)
                    //            {
                    //                markers.RemoveAt(x);
                    //            }
                    //        }

                    //        markers.Insert(x, mknew);
                    //        c -= (i - x);
                    //        i = x;
                    //    }
                    //}

                }

            }
        }

        /// <summary>
        /// Translate <paramref name="activas"/> to access modifiers.
        /// </summary>
        /// <param name="activas"></param>
        /// <returns></returns>
        private AccessModifiers ActivasToAccessModifiers(Dictionary<string, bool> activas)
        {
            var test = new string[] { "public", "private", "internal", "protected" };

            var sb = new StringBuilder();

            foreach (var t in test)
            {
                if (activas[t])
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(TextTools.TitleCase(t));
                }

            }

            if (Enum.TryParse<AccessModifiers>(sb.ToString(), out AccessModifiers result))
            {
                return result;
            }

            return AccessModifiers.None;

        }

        /// <summary>
        /// Calculate the line column from the present position.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ColumnFromHere(char[] chars, int pos)
        {

            int c = 0;
            int i;

            for (i = pos - 1; i >= 0; i--)
            {

                var ch = chars[i];
                if (ch == '\n') return c;

                c++;
            }

            return pos;
        }

        #endregion

    }

}
