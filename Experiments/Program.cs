using DataTools.Code.CS.Filtering;
using DataTools.Code.Markers;
using DataTools.CSTools;

using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Experiments
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var testr = "🂢";

            foreach (var che in testr)
            {
                if (char.IsHighSurrogate(che)) Console.WriteLine($"High Surrogate {((int)che):x2} {che}");
                if (char.IsLowSurrogate(che)) Console.WriteLine($"Low Surrogate {((int)che):x2} {che}");
            }

            var ba = Encoding.UTF8.GetBytes(testr);

            if (char.IsSurrogatePair(testr, 0))
            {
                Console.WriteLine("bingo");
            }

            //var path = @"E:\Projects\Personal Projects\Repos\DataTools\Win32\DataTools.Win32.Memory\VirtualMemPtr.cs";

            //var tool = new MSBuildTool();
            //var str = tool.FindILDasm();
            //var data = File.ReadAllText(path);

            var file = CSCodeFile.LoadFromFile(@"E:\Projects\Personal Projects\Repos\DataTools\Core\DataTools.Graphics\Structs\HUE.cs");

            var filter = new CSProjectDisplayChain<CSMarker, ObservableMarkerList<CSMarker>>();

            var results = filter.ApplyFilter(file.Markers);

            //var first = file.ScanMarker(file.Markers, (m) =>
            //{
            //    return m.IsExtern == true;
            //});

            //CodeFilterOptions cfo = new CodeFilterOptions(first, true);

            //var b = cfo.Validate(first);

            //Console.WriteLine($"{b}");

            //var cfr = new CodeFilterRule<CSMarker, ObservableMarkerList<CSMarker>>(new CodeFilterOptions() { IsExtern = true });

            //var results = cfr.ApplyFilter(file.Markers);

            //WriteResult(results);

            //Console.Read();
            //var outdir = @"C:\Users\theim\Desktop\Spasms";
            //file.OutputMarkers(outdir);

            //var markers = file.GetMarkersForCommit();
        }

        private static void WriteResult(IEnumerable results)
        {
            foreach (IMarker elem in results)
            {
                if (elem.Kind == MarkerKind.XMLDoc)
                {
                    Console.Write(elem.Content);
                }
                else
                {
                    Console.WriteLine(elem);
                }
                if (elem.Children != null)
                {
                    WriteResult(elem.Children);
                }
            }
        }
    }
}