using CSRefactorCurio.ViewModels;

using DataTools.Code.Filtering;
using DataTools.Code.Markers;
using DataTools.Code.Project;
using DataTools.Code.Reporting;
using DataTools.CSTools;

using System.Collections.Generic;
using System.Linq;

namespace CSRefactorCurio.Reporting
{
    internal abstract class CSReportBase<T> : ReportBase<T> where T : IReportNode, new()
    {
        protected CSReportBase(ISolution solution) : base(solution)
        {
        }
    }

    internal static class ReportHelper
    {
        public static Dictionary<string, List<CSCodeFile>> CountFilesForNamespaces(Dictionary<string, List<INamespace>> allfqn)
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

        public static List<CSReference<CSMarker>> GetReferences(CurioExplorerSolution sln, Dictionary<string, List<INamespace>> allfqn)
        {
            return GetReferences(sln.Projects, allfqn);
        }

        public static List<CSReference<CSMarker>> GetReferences(IEnumerable<IProjectElement> projects, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<CSReference<CSMarker>>();

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

        public static List<CSReference<CSMarker>> GetReferences(CurioProject proj, Dictionary<string, List<INamespace>> allfqn)
        {
            return GetReferences(proj.RootFolder, allfqn);
        }

        public static List<CSReference<CSMarker>> GetReferences(CSDirectory dir, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<CSReference<CSMarker>>();

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

        public static List<CSReference<CSMarker>> GetReferences(CSCodeFile file, Dictionary<string, List<INamespace>> allfqn)
        {
            var lt = new List<CSReference<CSMarker>>();

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

        private static List<CSReference<CSMarker>> GetReferences(List<string> usings, CSMarker marker, Dictionary<string, List<INamespace>> allfqn)
        {
            var nsi = usings;
            if (nsi == null || nsi.Count == 0) return new List<CSReference<CSMarker>>();

            List<string> combos = new List<string>();
            var lt = new List<CSReference<CSMarker>>();

            if (marker.UnknownWords != null)
            {
                foreach (var s2 in marker.UnknownWords)
                {
                    combos.Add(s2);

                    foreach (var s in nsi)
                    {
                        if (s != "")
                        {
                            combos.Add(s + "." + s2);
                        }
                    }
                }

                foreach (var s in combos)
                {
                    if (allfqn.TryGetValue(s, out var ns))
                    {
                        foreach (var item2 in ns)
                        {
                            if (item2 is CSMarker marker2)
                            {
                                lt.Add(new CSReference<CSMarker>(marker, marker2));
                            }
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

        public static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> namespaces, ItemFilterFunc filter) where T : INamespace
        {
            var dict = new Dictionary<string, List<INamespace>>();
            return AllFullyQualifiedNames(namespaces, dict, filter);
        }

        public static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> namespaces, MarkerKind[] filter = null) where T : INamespace
        {
            if (filter == null || filter.Length == 0)
            {
                filter = DefaultOrders.DefaultFQNFilter;
            }

            var dict = new Dictionary<string, List<INamespace>>();

            return AllFullyQualifiedNames(namespaces, dict, (t) =>
            {
                if (t is IMarker marker)
                {
                    return filter.Contains(marker.Kind);
                }
                else
                {
                    return true;
                }
            });
        }

        private static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> items, Dictionary<string, List<INamespace>> currDict, ItemFilterFunc filter) where T : INamespace
        {
            foreach (var nsobj in items)
            {
                if (filter(nsobj))
                {
                    if (!currDict.TryGetValue(nsobj.FullyQualifiedName, out List<INamespace> myf))
                    {
                        myf = new List<INamespace>();
                        currDict.Add(nsobj.FullyQualifiedName, myf);
                    }

                    if (!myf.Contains(nsobj))
                    {
                        myf.Add(nsobj);
                    }
                }

                if (nsobj is CSNamespace ns)
                {
                    AllFullyQualifiedNames(ns.Markers, currDict, filter);
                    AllFullyQualifiedNames(ns.Namespaces, currDict, filter);
                }
                else if (nsobj is IMarker mk)
                {
                    AllFullyQualifiedNames((IEnumerable<IMarker>)mk.Children, currDict, filter);
                }
            }

            return currDict;
        }
    }
}