using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.OLE.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.Reporting
{

    public static class ReportHelper
    {
        internal static Dictionary<string, List<CSCodeFile>> CountFilesForNamespaces(Dictionary<string, List<INamespace>> allfqn)
        {
            var outDict = new Dictionary<string, List<CSCodeFile>>();

            foreach (var kvp in allfqn)
            {
                var ns = kvp.Value;

                foreach (var item in ns)
                {
                    if (item is CSMarker marker)
                    {
                        if (!outDict.TryGetValue(item.Namespace, out var csFiles))
                        {
                            csFiles = new List<CSCodeFile>();
                            outDict.Add(item.Namespace, csFiles);
                        }

                        var file = marker.HomeFile as CSCodeFile;

                        if (file != null)
                        {
                            if (!csFiles.Contains(file))
                            {
                                csFiles.Add(file);
                            }
                        }
                    }
                }
            }
            
            var keys = outDict.Keys.ToArray();

            foreach (var key in keys)
            {
                if (outDict[key].Count == 0)
                {
                    outDict.Remove(key);
                }
            }

            return outDict; 
        }


        internal static List<(CSMarker, CSMarker)> GetReferences(CurioExplorerSolution sln, Dictionary<string, List<INamespace>> allfqn)
        {
            return GetReferences(sln.Projects, allfqn);
        }

        public static List<(CSMarker, CSMarker)> GetReferences(IEnumerable<IProjectElement> projects, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<(CSMarker, CSMarker)>();

            foreach (var elem in projects)
            {
                if (elem is CurioProject proj)
                {
                    lt.AddRange(GetReferences(proj, allfqn));   
                }
                else if (elem is CSSolutionFolder fld)
                {
                    lt.AddRange(GetReferences(fld.Children, allfqn));
                }
            }

            return lt;
        }

        public static List<(CSMarker, CSMarker)> GetReferences(CurioProject proj, Dictionary<string, List<INamespace>> allfqn)
        {
            return GetReferences(proj.RootFolder, allfqn);
        }

        public static List<(CSMarker, CSMarker)> GetReferences(CSDirectory dir, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<(CSMarker, CSMarker)>();

            foreach (var file in dir.Files)
            {
                lt.AddRange(GetReferences(file, allfqn));
            }

            foreach (var dir2 in dir.Directories)
            {
                lt.AddRange(GetReferences(dir2, allfqn));
            }

            return lt;
        }

        public static List<(CSMarker, CSMarker)> GetReferences(CSCodeFile file, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<(CSMarker, CSMarker)>();

            var usings = new List<string>();

            foreach (var marker in file.Markers)
            {
                usings.AddRange(marker.ExtractAllUsings());
            }

            foreach (var marker in file.Markers)
            {
                lt.AddRange(GetReferences(usings, marker, allfqn));
            }

            return lt;
        }

        private static List<(CSMarker, CSMarker)> GetReferences(List<string> usings, CSMarker marker, Dictionary<string, List<INamespace>> allfqn)
        {
            var nsi = usings;
            if (nsi == null || nsi.Count == 0) return new List<(CSMarker, CSMarker)>();

            List<string> combos = new List<string>();

            foreach (var s in nsi)
            {
                if (marker.UnknownWords == null) continue;
                foreach (var s2 in marker.UnknownWords)
                {
                    if (s != "")
                    {
                        combos.Add(s + "." + s2);
                    }
                }
            }

            var lt = new List<(CSMarker, CSMarker)>();

            foreach(var s in combos)
            {
                if (allfqn.TryGetValue(s, out var ns))
                {

                    foreach (var item2 in ns)
                    {
                        if (item2 is CSMarker marker2)
                        {
                            lt.Add((marker, marker2));
                            
                        }
                    }
                }
            }

            foreach (var child in marker.Children)
            {
                lt.AddRange(GetReferences(usings, child, allfqn));
            }

            return lt;
        }

        public static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> namespaces) where T : INamespace
        {
            var dict = new Dictionary<string, List<INamespace>>();
            return AllFullyQualifiedNames(namespaces, dict);
        }

        private static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> items, Dictionary<string, List<INamespace>> currDict) where T : INamespace
        {
            foreach (var nsobj in items)
            {

                if (nsobj is IMarker mkt)
                {
                    switch (mkt.Kind)
                    {
                        case MarkerKind.Class:
                        case MarkerKind.Interface:
                        case MarkerKind.Struct:
                        case MarkerKind.Enum:
                        case MarkerKind.Record:
                        case MarkerKind.Event:
                        case MarkerKind.Delegate:
                            break;

                        default:
                            continue;
                    }
                }

                if (!currDict.TryGetValue(nsobj.FullyQualifiedName, out List<INamespace> myf))
                {
                    myf = new List<INamespace>();
                    currDict.Add(nsobj.FullyQualifiedName, myf);
                }

                if (!myf.Contains(nsobj))
                {
                    myf.Add(nsobj);
                }
                
                if (nsobj is CSNamespace ns)
                {
                    AllFullyQualifiedNames(ns.Markers, currDict);
                    AllFullyQualifiedNames(ns.Namespaces, currDict);
                }
                else if (nsobj is IMarker mk)
                {
                    AllFullyQualifiedNames((IEnumerable<IMarker>)mk.Children, currDict);
                }
            }

            return currDict;
        }
    }

    public class HeaviestReferencesReport : CSReportBase
    {
        public override string ReportName { get; }
        public override string AssociatedReason { get; }
        public override IList<IReportNode<CSMarker>> Reports { get; }
        public override int ReportId { get; }

        public override void CompileReport(IList<IProjectElement> context)
        {
            foreach (var item in context)
            {
                if (item is CSMarker marker)
                {

                }
                else if (item is INamespace ns)
                {

                }
            }
        }

        public override void Sort()
        {
            throw new NotImplementedException();
        }
    }

}
