using DataTools.Code.CS;
using DataTools.Code.Markers;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataTools.Code
{
    /// <summary>
    /// Code Parser Output File
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/></typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
    internal class OutputFile<TMarker, TList> where TMarker : IMarker, new() where TList : IMarkerList<TMarker>, new()
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
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
        /// <param name="path">The path of the new file.</param>
        /// <param name="file">The <see cref="AtomicGenerationInfo{TMarker, TList}"/> information.</param>
        /// <param name="lines">The original file split into lines.</param>
        /// <param name="sepDirs">True to put different kinds of items in separate directories.</param>
        /// <param name="parser">The code parser instance.</param>
        /// <returns>A new output file.</returns>
        public static OutputFile<TI, TL> NewFile<TI, TL>(string path, AtomicGenerationInfo<TI, TL> file, string[] lines, bool sepDirs, CodeParserBase<TI, TL> parser = null) where TI : IMarker<TI, TL>, new() where TL : IMarkerList<TI>, new()
        {
            return NewFile(path, file.Markers[0].Kind, file.Markers[0].Name, FormatOutputText<TI, TL>(file.Markers, lines, file.PreambleEnd, file.PreambleBegin), sepDirs, parser);
        }

        /// <summary>
        /// Create a new file from the given parameters.
        /// </summary>
        /// <typeparam name="TI">The type of <see cref="IMarker"/></typeparam>
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
        /// <param name="path">The path of the new file.</param>
        /// <param name="mkind">Specifies the top-level marker kind (for determining the destination folder)</param>
        /// <param name="name">The name of the new file (without extension)</param>
        /// <param name="text">The contents of the new file</param>
        /// <param name="sepDirs">True to put different kinds of items in separate directories.</param>
        /// <param name="cs">An optional reference <see cref="CSCodeParser{TMarker, TList}"/> with configuration information.</param>
        /// <returns>A new output file.</returns>
        public static OutputFile<TI, TL> NewFile<TI, TL>(string path, CodeElementType mkind, string name, string text, bool sepDirs, CodeParserBase<TI, TL> cs = null) where TI : IMarker<TI, TL>, new() where TL : IMarkerList<TI>, new()
        {
            path = path.Trim().Trim('\\');
            string kind = "";
            switch (mkind)
            {
                case CodeElementType.Class:
                    kind = cs?.ClassDirName ?? "Classes";
                    break;

                case CodeElementType.Interface:
                    kind = cs?.InterfaceDirName ?? "Interfaces";
                    break;

                case CodeElementType.Struct:
                    kind = cs?.StructDirName ?? "Structs";
                    break;

                case CodeElementType.Enum:
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
        /// Create a simple <see cref="OutputFile{TMarker, TList}"/> instance with just the specified filename and text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static OutputFile<TMarker, TList> NewFile(string filename, string text)
        {
            return new OutputFile<TMarker, TList>
            {
                Text = text,
                Filename = filename
            };
        }

        /// <summary>
        /// Format the specified markers into a text for saving to a new class file.
        /// </summary>
        /// <typeparam name="TI">The type of <see cref="IMarker"/></typeparam>
        /// <typeparam name="TL">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
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

                t += marker.FormatContents();

                //for (var i = marker.StartLine - 1; i < marker.EndLine; i++)
                //{
                //    if (t != "") t += "\r\n";
                //    t += lines[i];
                //}

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
            var sb = new StringBuilder();

            for (var i = preambleFrom; i < preambleTo; i++)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                sb.Append(lines[i]);
            }

            return sb.ToString();
        }
    }
}