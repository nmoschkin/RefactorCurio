using DataTools.CSTools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiments
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\theim\Desktop\Projects\Personal Projects\Repos\RefactorCurio\CSRefactorCurio\DataTools\FileSystemMonitor.cs";


            var data = File.ReadAllText(path);


            var file = CSCodeFile.LoadFromFile(path);




        }
    }
}
