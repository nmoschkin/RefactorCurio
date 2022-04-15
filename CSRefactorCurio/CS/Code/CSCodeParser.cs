using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DataTools.Text;

namespace DataTools.CSTools
{
    
    public class WordObject
    {
        public string Word { get; set; }

        public int Line { get; set; }

        public static List<WordObject> LinesToWords(string[] lines)
        {
            WordObject o;
            List<WordObject> ret = new List<WordObject>();
            int c = 0;

            foreach (var s in lines)
            {
                var words = TextTools.Words(s, SkipQuotes: true);

                foreach (var w in words)
                {
                    o = new WordObject()
                    {
                        Word = w,
                        Line = c
                    };

                    ret.Add(o);
                    o = null;
                }

                c++;
            }

            return ret;
        }

        public override string ToString()
        {
            return Word;
        }

    }

    public class RenderedFile
    {
        public int PreambleBegin { get; set; }

        public int PreambleEnd { get; set; }

        public string OriginalFile { get; set; }

        public IList<Marker> Markers { get; set; }

        public IList<string> Lines { get; set; }

    }

    public class Marker
    {
        public RenderedFile RenderedFile { get; set; }

        public int StartLine { get; set; }


        public int EndLine { get; set; }

        public string Name { get; set; }

        public string Generics { get; set; }

        public string Kind { get; set; }

        public string Namespace { get; set; }

        public List<Marker> Markers { get; protected internal set; }

        public List<WordObject> Content { get; } = new List<WordObject>();

        public override string ToString()
        {
            return $"{Kind} {Name}{Generics}, Line: {StartLine} to {EndLine}";
        }

    }

    public class OutputFile
    {
        public string Text { get; set; }

        public string Filename { get; set; }

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

        public static OutputFile NewFile(string path, RenderedFile file, string[] lines, bool sepDirs, CSCodeParser parser = null)
        {
            return NewFile(path, file.Markers[0].Kind, file.Markers[0].Name, FormatOutputText(file.Markers, lines, file.PreambleEnd, file.PreambleBegin), sepDirs, parser);
        }

        public static OutputFile NewFile(string path, string type, string name, string text, bool sepDirs, CSCodeParser cs = null)
        {
            path = path.Trim().Trim('\\');

            switch (type)
            {
                case "class":
                    type = cs?.ClassDirName ?? "Classes";
                    break;

                case "interface":
                    type = cs?.InterfaceDirName ?? "Interfaces";
                    break;

                case "struct":
                    type = cs?.StructDirName ?? "Structs";
                    break;

                case "enum":
                    type = cs?.EnumDirName ?? "Enums";
                    break;

            }
            if (sepDirs && !string.IsNullOrEmpty(type))
            {
                return new OutputFile
                {
                    Text = text,
                    Filename = $"{path}\\{type}\\{name}.cs"
                };
            }
            else
            {
                return new OutputFile
                {
                    Text = text,
                    Filename = $"{path}\\{name}.cs"
                };

            }
        }


        public static OutputFile NewFile(string filename, string text)
        {
            return new OutputFile
            {
                Text = text,
                Filename = filename
            };
        }

        public static string FormatOutputText(IList<Marker> markers, string[] lines, int preambleTo = -1, int preambleFrom = 0)
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
    public class CSCodeParser
    {
        protected string[] lines = null;
        protected string text = null;

        protected int preambleTo = 0;

        protected List<Marker> markers = new List<Marker>();
        protected RenderedFile mfile = null;

        protected List<string> lastErrors = new List<string>();

        protected string filename = null;

        protected string[] outputFiles = new string[0];
                
        public virtual string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        public virtual string[] OutputFiles => outputFiles;

        public virtual string InterfaceDirName { get; set; } = "Contracts";

        public virtual string ClassDirName { get; set; } = "";

        public virtual string EnumDirName { get; set; } = "Enums";

        public virtual string StructDirName { get; set; } = "Structs";

        public virtual bool SeparateDirs { get; set; } = true;

        public virtual bool ParseSuccess { get; protected set; } = false;

        public virtual string Filename
        {
            get => filename;
            protected set => filename = value;
        }

        public virtual IReadOnlyList<string> Errors
        {
            get => lastErrors;
        }

        public virtual void LoadFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);

            lastErrors = new List<string>();
            Filename = filename;
            OutputPath = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "\\");

            Parse(File.ReadAllText(filename));
        }

        public void Refresh()
        {
            Parse(File.ReadAllText(Filename));
        }

        protected CSCodeParser()
        {
        }

        public CSCodeParser(string text)
        {
            Parse(text);
        }

        public static CSCodeParser LoadFromFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            var res = new CSCodeParser();
            res.LoadFile(filename);
            return res;
        }

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

            List<Marker> seen = new List<Marker>();
            
            if (mfile == null) return false;
            if (markers == null) return false;

            foreach (var marker in mfile.Markers)
            {
                if (seen.Contains(marker)) continue;

                var mlist = new List<Marker>();

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

                mlist.Sort((a, b) =>
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

                if (mlist.Count == 0) continue;
                seen.AddRange(mlist);
                var mf = new RenderedFile()
                {
                    Lines = mfile.Lines,
                    Markers = mlist,
                    PreambleBegin = mfile.PreambleBegin,
                    PreambleEnd = mfile.PreambleEnd
                };

                var file = OutputFile.NewFile(path, mf, lines, SeparateDirs, this);
                file.Write();
            }

            return true;
        }

        public virtual List<Marker> Markers => markers;

        public virtual int PreambleTo => preambleTo;

        public virtual string Text => text;


        public virtual string[] Lines => lines;

        public override string ToString()
        {
            return Filename;
        }

        protected virtual bool Parse(string text)
        {
            ParseSuccess = false;

            this.text = text;
            this.lines = text.Replace("\r\n", "\n").Split('\n');

            try
            {
                markers = ParseCSCodeFile(text);

                var cf = new RenderedFile()
                {
                    PreambleBegin = 0,
                    PreambleEnd = preambleTo,
                    Markers = markers ?? new List<Marker>()
                };

                mfile = cf;

                foreach (var marker in markers)
                {
                    marker.RenderedFile = cf;
                }

                ParseSuccess = true;
            }
            catch (SyntaxErrorException ex)
            {
                lastErrors.Add(ex.Message);
            }

            return ParseSuccess;
        }


        protected virtual List<Marker> ParseCSCodeFile(string text)
        {
            bool inthing = false;
            bool indelg = false;

            int level = 0;
            int startL = 0;

            char[] allowed = (TextTools.AlphaNumericChars + "_-").ToCharArray();
            char[] nsallowed = (TextTools.AlphaNumericChars + "_-.").ToCharArray();

            var scans = new List<char[]>();
            var scans2 = new List<char[]>();

            scans.Add("class".ToCharArray());
            scans.Add("interface".ToCharArray());
            scans.Add("enum".ToCharArray());
            scans.Add("struct".ToCharArray());
            scans.Add("namespace".ToCharArray());
            scans.Add("delegate".ToCharArray());

            string kind = null, name = null, generics = null;
            string kind2 = null, name2 = null, generics2 = null;

            string namesp = null;

            int linestart = 0;
            int i, c;
            int line = 0;
            int j, d;
            int startLine = 0;
            int endLine = 0;
            int startPos, endPos;

            int startLine2 = 0;
            int endLine2 = 0;
            int startPos2 = 0, endPos2 = 0;
            bool marker2 = false;
            bool firstNs = true;
            int lreal = 0;
            int pre = 0;
            var reg = new Regex(@"^(.+)\s+(\w+)$");
            var reg2 = new Regex(@"^(.+)\s+(\w+)\s*(\(|\})(.+)(\(|\})$");
            var reggen = new Regex(@"^(.+)\s+(\w+)<(.+)>$");

            List<Marker> markers = new List<Marker>();
            Marker mark;
            Marker mark2 = null;

            char[] input = text.ToCharArray();

            var totalLines = input.Count((co) => co == '\n');
            c = input.Length;


            for (i = 0; i < c; i++)
            {
                if (input[i] == '\r' || input[i] == '\t') continue;

                if (input[i] == '\n')
                {
                    line++;
                    //if (line >= totalLines) throw new SyntaxErrorException();
                    linestart = i + 1;
                    continue;
                }

                // Comments

                if (input[i] == '/')
                {
                    if (i >= c - 1) break;

                    if (input[i + 1] == '/')
                    {
                        i += 2;
                        while (input[i] != '\n' && i < c - 1)
                        {
                            i++;
                        }

                        if (input[i] == '\n')
                        {
                            linestart = i;
                            line++;
                        }

                        if (line >= totalLines) break;

                        continue;
                    }
                    else if (input[i + 1] == '*')
                    {
                        i += 2;
                        while (i < c - 2)
                        {
                            if (input[i] == '\n')
                            {
                                line++;
                                i++;
                                linestart = i;

                                if (line >= totalLines) throw new SyntaxErrorException($"Comment has no end commend string '*/' at line {line}");

                                continue;
                            }

                            if (input[i] == '*' && input[i + 1] == '/')
                            {
                                i++;
                                break;
                            }

                            i++;
                        }

                        continue;
                    }
                }

                // Character literals
                if (input[i] == '\'')
                {
                    if (i > c - 1) throw new SyntaxErrorException();

                    bool chi = false;
                    for (j = i + 1; j < c; j++)
                    {
                        if (input[j] == '\r' || input[j] == '\n') throw new SyntaxErrorException();

                        if (input[j] == '\\')
                        {
                            if (input[j - 1] != '\\')
                            {
                                chi = true;
                            }
                            else
                            {
                                chi = false;
                            }
                        }
                        else if (input[j] == '\'')
                        {
                            if (input[j - 1] != '\\' || !chi)
                            {
                                // end of the string
                                i = j;
                                break;
                            }
                        }
                    }
                }


                // Quotes 
                if (input[i] == '\"')
                {
                    var sbtemp = TextTools.QuoteFromHere(input, i, ref line, out int? steepPos, out int? eepPos, '$', '@', '{', '}');

                    if (eepPos != null)
                    {
                        i = (int)eepPos;
                        continue;
                    }

                    //if (i > c - 1) throw new SyntaxErrorException("Quote at end of file.");
                    //var sbtemp = new StringBuilder();
                    //sbtemp.Append(input[i]);
                    //bool inlit = false;
                    //bool intrans = false;
                    //bool atrans = false;
                    //int transl = 0;
                    //List<bool> qtrans = null;
                    //List<bool> translit = null;

                    //if (i > 0 && input[i - 1] == '@') inlit = true;
                    //if (i > 0 && input[i - 1] == '$')
                    //{
                    //    intrans = true;
                    //    qtrans = new List<bool>();
                    //    translit = new List<bool>();
                    //    qtrans.Add(true);
                    //    translit.Add(false);
                    //}

                    //for (j = i + 1; j < c; j++)
                    //{
                    //    sbtemp.Append(input[j]);

                    //    if (intrans && input[j] == '{')
                    //    {
                    //        transl++;
                    //        qtrans.Add(false);
                    //        translit.Add(false);
                    //    }
                    //    else if (intrans && input[j] == '}')
                    //    {
                    //        if (!qtrans[transl])
                    //        {
                    //            qtrans.RemoveAt(transl);
                    //            translit.RemoveAt(transl);

                    //            transl--;
                    //        }
                    //    }
                    //    else if (input[j] == '\n')
                    //    {
                    //        int oline = line;
                    //        //throw new SyntaxErrorException();
                    //        line++;
                    //        j++;
                    //        linestart = j;
                    //        if (line >= totalLines) throw new SyntaxErrorException($"No closed quotes found for quote starting at line {oline}");
                    //        continue;
                    //    }
                    //    if (!inlit && input[j] == '\\')
                    //    {
                    //        if (intrans)
                    //        {
                    //            if (qtrans[transl])
                    //            {
                    //                j += 1;
                    //                continue;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            j += 1;
                    //            continue;
                    //        }
                    //    }
                    //    else if (input[j] == '\"')
                    //    {
                    //        if (intrans)
                    //        {
                    //            if (translit[transl] && j < c - 1)
                    //            {
                    //                if (input[j + 1] == '\"')
                    //                {
                    //                    j++;
                    //                    continue;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                qtrans[transl] = !qtrans[transl];
                    //                if (qtrans[0] == false)
                    //                {
                    //                    i = j;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //if (input[j - 1] != '\\')
                    //            //{
                    //            //    // end of the string
                    //            //    i = j;
                    //            //    break;
                    //            //}

                    //            if (inlit)
                    //            {
                    //                if (j < c - 1)
                    //                {
                    //                    if (input[j + 1] == '\"')
                    //                    {
                    //                        j++;
                    //                        continue;
                    //                    }
                    //                }

                    //                i = j;
                    //                break;
                    //            }
                    //            else
                    //            {
                    //                i = j;
                    //                break;
                    //            }

                    //        }
                    //    }
                    //}

                    Console.WriteLine($"{sbtemp} Line: {line}");

                }

                if (indelg && input[i] == ';')
                {

                    endPos = i;
                    endLine = line;

                    mark = new Marker()
                    {
                        StartLine = lreal,
                        EndLine = endLine,
                        Kind = kind,
                        Name = name,
                        Generics = generics,
                        Namespace = namesp
                    };

                    markers.Add(mark);
                    indelg = inthing = false;

                    lreal = line + 1;
                    generics = name = kind = null;
                }

                if (inthing && !indelg)
                {
                    if (input[i] == ';' && mark2 != null)
                    {
                        
                        if (i + 1 > startPos2)
                            startPos2 = i + 1;

                        mark2.EndLine = line;
                    }

                    if (input[i] == '{')
                    {
                        level++;
                        if (level == startL + 1)
                        {
                            startPos2 = i + 1;
                            startLine2 = line;
                        }
                        else if (level == startL + 2)
                        {
                            var scan = new string(input, startPos2, i - startPos2).Replace("\n", "").Replace("\r", "").Trim();

                            var regcheck = reg.Match(scan);
                            var regcheck2 = reg2.Match(scan);

                            marker2 = true;
                            mark2 = new Marker();
                            mark2.StartLine = line;
                            mark2.Namespace = namesp;
                            if (regcheck != null && regcheck.Success)
                            {



                            }

                        }
                    }
                    else if (input[i] == '}')
                    {
                        level--;
                        if (level == startL + 1)
                        {
                            marker2 = false;
                            startPos2 = i + 1;
                        }
                        else if (level == startL)
                        {
                            endPos = i;
                            endLine = line;

                            mark = new Marker()
                            {
                                StartLine = lreal,
                                EndLine = endLine,
                                Kind = kind,
                                Name = name,
                                Generics = generics,
                                Namespace = namesp
                            };

                            markers.Add(mark);
                            inthing = false;

                            lreal = line + 1;
                            generics = name = kind = null;
                        }
                    }

                    continue;
                }


                if (char.IsWhiteSpace(input[i])) continue;

                if (char.IsLetter(input[i]))
                {

                    foreach (var scan in scans)
                    {
                        var sc = new string(scan);

                        if (scan[0] == input[i])
                        {
                            int ss = line;

                            for (j = 0; j < scan.Length; j++)
                            {
                                if (scan[j] != input[i]) break;
                                i++;
                            }

                            if (sc == "namespace")
                            {

                                if (j == scan.Length && i < c - 1 && !nsallowed.Contains(input[i]))
                                {
                                    while (!nsallowed.Contains(input[i]) && i < c - 1) i++;

                                    int sn = i;

                                    while (nsallowed.Contains(input[i]) && i < c - 1) i++;
                                    int en = i;

                                    namesp = text.Substring(sn, en - sn);
                                    if (firstNs)
                                    {
                                        pre = ss - 1;
                                        lreal = pre + 3;
                                    }
                                    firstNs = false;

                                    break;
                                }

                            }
                            else
                            {
                                if (j == scan.Length && i < c - 1 && !allowed.Contains(input[i]))
                                {
                                    while (!allowed.Contains(input[i]) && i < c - 1) i++;

                                    int sn = i;

                                    while (allowed.Contains(input[i]) && i < c - 1) i++;
                                    int en = i;

                                    name = text.Substring(sn, en - sn);

                                    if (input[i] == ' ')
                                    {
                                        while (input[i] == ' ') i++;
                                    }

                                    if (input[i] == '<')
                                    {
                                        int genpart = i;
                                        int genend = i;
                                        int genlevel = 0;
                                        while (true)
                                        {
                                            if (input[i] == '<') genlevel++;
                                            else if (input[i] == '>') genlevel--;

                                            if (genlevel == 0 || i >= c - 1) break;
                                            i++;

                                        }

                                        i++;
                                        genend = i;
                                        generics = text.Substring(genpart, genend - genpart);
                                    }

                                    if (sc == "delegate") indelg = true;

                                    if (indelg && i < c)
                                    {
                                        while (!allowed.Contains(input[i]) && i < c - 1) i++;
                                        sn = i;

                                        while (allowed.Contains(input[i]) && i < c - 1) i++;
                                        en = i;

                                        name = text.Substring(sn, en - sn);
                                    }

                                    kind = new string(scan);
                                    startLine = line;
                                    startPos = linestart;
                                    inthing = true;
                                    startL = level;

                                    break;
                                }

                            }
                        }
                    }

                    continue;
                }

            }

            if (inthing) throw new SyntaxErrorException($"No closing brace for entity starting at line {startLine}");

            preambleTo = pre;
            return markers;

        }
    }

    //public class SyntaxErrorException : Exception
    //{

    //    public SyntaxErrorException()
    //    {
    //    }

    //    public SyntaxErrorException(string message) : base(message)
    //    {
    //    }
    //}
}
