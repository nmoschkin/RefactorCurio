using CSRefactorCurio.Globalization.Resources;
using CSRefactorCurio.ViewModels;

using DataTools.CSTools;
using DataTools.SortedLists;

using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.OLE.Interop;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.Reporting
{

    public delegate bool ItemFilterFunc(INamespace item);

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

        public static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> namespaces, ItemFilterFunc filter) where T : INamespace
        {
            var dict = new Dictionary<string, List<INamespace>>();
            return AllFullyQualifiedNames(namespaces, dict, filter);
        }

        public static Dictionary<string, List<INamespace>> AllFullyQualifiedNames<T>(IEnumerable<T> namespaces, MarkerKind[] filter = null) where T : INamespace
        {
            if (filter == null || filter.Length == 0)
            {
                filter = new[]
                {
                    MarkerKind.Namespace,
                    MarkerKind.Class,
                    MarkerKind.Interface,
                    MarkerKind.Struct,
                    MarkerKind.Record,
                    MarkerKind.Enum,
                    MarkerKind.Method,
                    MarkerKind.Property,
                    MarkerKind.Delegate,
                    MarkerKind.Const
                };
            }

            var dict = new Dictionary<string, List<INamespace>>();
            
            return AllFullyQualifiedNames(namespaces, dict, (t) => {
                
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

    public class HeaviestReferencesReport : ReportBase<ReportNode<INamespace>>
    {
        [Browsable(true)]
        public override string ReportName { get; } = AppResources.REPORT_MOST_REFERENCED_OBJECTS;

        [Browsable(true)]
        public override string AssociatedReason { get; }

        [Browsable(true)]
        public override int ReportId { get; } = 1;

        public HeaviestReferencesReport(ISolution solution, string associated) : base(solution)
        {
            AssociatedReason = associated;
        }
        public HeaviestReferencesReport(ISolution solution) : this(solution, "DEFAULT")
        {
        }

        public override void CompileReport<T>(IList<T> context)
        {
            var allFQN = ReportHelper.AllFullyQualifiedNames(context);

            var allref = ReportHelper.GetReferences(Solution.Projects, allFQN);

            allref.Sort((a, b) =>
            {
                var c = string.Compare(a.Item2.FullyQualifiedName, b.Item2.FullyQualifiedName);
                if (c == 0)
                {
                    c = string.Compare(a.Item2.Generics, b.Item2.Generics);

                    if (c == 0)
                    {
                        c = string.Compare(a.Item1.FullyQualifiedName, b.Item1.FullyQualifiedName);

                        if (c == 0)
                        {
                            c = string.Compare(a.Item2.Generics, b.Item2.Generics);
                        }

                    }

                }

                if (c == 0)
                {
                    if (a != b)
                    {
                        return -1;
                    }
                }

                return c;
            });

            var rpts = new List<ReportNode<INamespace>>();

            CSMarker curr = null;

            List<CSMarker> markers = new List<CSMarker>();

            foreach (var item in allref)
            {
                if (curr != item.Item2)
                {
                    if (markers.Count > 0)
                    {
                        var rpt = new ReportNode<INamespace>()
                        {
                            AssociatedList = markers.ToArray(),
                            Element = curr
                        };

                        rpts.Add(rpt);
                        markers = new List<CSMarker>();
                    }
                    
                    curr = item.Item2;
                }

                if (!markers.Contains(item.Item1)) markers.Add(item.Item1);
            }

            if (curr != null)
            {
                if (markers.Count > 0)
                {
                    var rpt = new ReportNode<INamespace>()
                    {
                        AssociatedList = markers.ToArray(),
                        Element = curr
                    };

                    rpts.Add(rpt);
                    markers = new List<CSMarker>();
                }
            }

            Reports = rpts;
            Sort();
        }

        public override void Sort()
        {
            QuickSort.Sort(Reports, (a, b) =>
            {
                if (a.AssociatedList.Count > b.AssociatedList.Count) return -1;
                if (a.AssociatedList.Count < b.AssociatedList.Count) return 1;
                return 0;
            });
        }
    }




    public class MostSpreadOutNamespacesReport : ReportBase<ReportNode<IProjectNode>>
    {

        [Browsable(true)]
        public override string ReportName { get; } = AppResources.REPORT_MOST_SPREAD_OUT;

        [Browsable(true)]
        public override string AssociatedReason { get; }
                
        [Browsable(true)]
        public override int ReportId { get; } = 1;

        public MostSpreadOutNamespacesReport(ISolution solution, string associated) : base(solution)
        {
            AssociatedReason = associated;
        }
        public MostSpreadOutNamespacesReport(ISolution solution) : this(solution, "DEFAULT")
        {
        }

        public override void CompileReport<T>(IList<T> context) 
        {
            var allFQN = ReportHelper.AllFullyQualifiedNames(context);

            var allref = ReportHelper.CountFilesForNamespaces(allFQN);

            var rpts = new List<ReportNode<IProjectNode>>();

            CSMarker curr = null;

            List<CSMarker> markers = new List<CSMarker>();

            foreach (var item in allref)
            {
                var rpt = new ReportNode<IProjectNode>()
                {
                    AssociatedList = new List<IProjectNode>(item.Value.Select((x) => (IProjectNode)x)),
                    Element = allFQN.Where((x) => x.Key == item.Key).First().Value.First(),
                };

                rpts.Add(rpt);
            }

            if (curr != null)
            {
                if (markers.Count > 0)
                {
                    var rpt = new ReportNode<IProjectNode>()
                    {
                        AssociatedList = markers.ToArray(),
                        Element = curr
                    };

                    rpts.Add(rpt);
                    markers = new List<CSMarker>();
                }
            }

            Reports = rpts;
            Sort();
        }

        public override void Sort()
        {
            QuickSort.Sort(Reports, (a, b) =>
            {
                if (a.AssociatedList.Count > b.AssociatedList.Count) return -1;
                if (a.AssociatedList.Count < b.AssociatedList.Count) return 1;
                return 0;
            });
        }
    }


}
