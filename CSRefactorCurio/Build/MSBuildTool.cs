using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataTools.CSTools
{
    internal class MSBuildTool
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        public const string FindMSBuildCmd = "\"%ProgramFiles(x86)%\\Microsoft Visual Studio\\Installer\\vswhere.exe\"";
        public const string FindMSBuildParams = "-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe";
        public const string FindILDasmParams = "-latest -prerelease -products * -requires Microsoft.Component.ILDasm -find MSBuild\\**\\Bin\\ILDasm.exe";

        private string msBuild;

        public MSBuildTool()
        {
            msBuild = FindMSBuild();
        }

        public bool RunMSBuild(CurioProject project)
        {
            var cmd = $"\"{project.ProjectRootPath}\\{project.ProjectFile}\" -t:Rebuild /property:Configuration=Debug /property:Platform=\"AnyCPU\"";

            var proc = new Process();

            proc.StartInfo.WorkingDirectory = project.ProjectRootPath;
            proc.StartInfo.FileName = msBuild;
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = false;

            proc.Start();
            proc.WaitForExit();

            return proc.ExitCode == 0;
        }

        public string FindILDasm()
        {
            return @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\ildasm.exe";
        }

        public string FindMSBuild()
        {
            var proc = new Process();

            proc.StartInfo.FileName = Environment.ExpandEnvironmentVariables(FindMSBuildCmd);
            proc.StartInfo.Arguments = FindMSBuildParams;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            proc.WaitForExit();

            var strm = proc.StandardOutput.ReadToEnd();

            return strm.Replace("\r", "").Replace("\n", "");
        }
    }
}