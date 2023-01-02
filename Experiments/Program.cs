using DataTools.CSTools;

using System;
using System.Linq;
using System.Text;

namespace Experiments
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var path = @"E:\Projects\Personal Projects\Repos\DataTools\Win32\DataTools.Win32.Memory\VirtualMemPtr.cs";

            //var tool = new MSBuildTool();
            //var str = tool.FindILDasm();
            //var data = File.ReadAllText(path);

            var file = CSCodeFile.LoadFromFile("E:\\Projects\\Personal Projects\\Repos\\RefactorCurio\\Experiments\\Class1.cs");

            //var outdir = @"C:\Users\theim\Desktop\Spasms";
            //file.OutputMarkers(outdir);

            //var markers = file.GetMarkersForCommit();
        }
    }
}