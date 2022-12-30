using CSRefactorCurio.Globalization.Resources;
using CSRefactorCurio.ViewModels;

using DataTools.Code.Markers;
using DataTools.Code.Project;
using DataTools.CSTools;
using DataTools.Essentials.SortedLists;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Reporting
{
    internal delegate bool ItemFilterFunc(INamespace item);

    internal class CSReference<T> where T : IMarker
    {
        public T ReferencedObject { get; set; }

        public T CallingObject { get; set; }

        public CSReference(T calling, T referenced)
        {
            ReferencedObject = referenced;
            CallingObject = calling;
        }

        public CSReference()
        {
        }

        public override string ToString()
        {
            return $"{CallingObject.Title} => {ReferencedObject.Title}";
        }
    }

    internal static class ReportHelper
    {
        /// <summary>
        /// Gets the default sort order for filters, lists, and rules.
        /// </summary>
        public static readonly MarkerKind[] DefaultSortOrder = new MarkerKind[] {
            MarkerKind.Interface,
            MarkerKind.Class,
            MarkerKind.Record,
            MarkerKind.Struct,
            MarkerKind.Enum,
            MarkerKind.Const,
            MarkerKind.Delegate,
            MarkerKind.Constructor,
            MarkerKind.Destructor,
            MarkerKind.Method,
            MarkerKind.Property,
            MarkerKind.Field,
            MarkerKind.Operator,
            MarkerKind.EnumValue,
            MarkerKind.FieldValue,
            MarkerKind.Event,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove
        };

        public static readonly MarkerKind[] DefaultFQNFilter = new[]
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
            MarkerKind.Const,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove,
        };

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
                filter = DefaultFQNFilter;
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

    internal class HeaviestReferencesReport : ReportBase<ReportNode<INamespace>>
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
            var so = (IList<MarkerKind>)ReportHelper.DefaultSortOrder;

            allref.Sort((a, b) =>
            {
                if (a.Equals(b)) return 0;

                var x = so.IndexOf(a.ReferencedObject.Kind);
                var y = so.IndexOf(a.ReferencedObject.Kind);

                if (x != -1 && y != -1)
                {
                    if (x < y) return -1;
                    if (y < x) return 1;
                }

                x = so.IndexOf(a.CallingObject.Kind);
                y = so.IndexOf(a.CallingObject.Kind);

                if (x != -1 && y != -1)
                {
                    if (x < y) return -1;
                    if (y < x) return 1;
                }

                var c = string.Compare(a.ReferencedObject.FullyQualifiedName, b.ReferencedObject.FullyQualifiedName);

                if (c == 0)
                {
                    c = string.Compare(a.ReferencedObject.Generics, b.ReferencedObject.Generics);

                    if (c == 0)
                    {
                        c = string.Compare(a.CallingObject.FullyQualifiedName, b.CallingObject.FullyQualifiedName);

                        if (c == 0)
                        {
                            c = string.Compare(a.ReferencedObject.Generics, b.ReferencedObject.Generics);
                        }
                    }
                }

                return c;
            });

            var rpts = new List<ReportNode<INamespace>>();

            CSMarker curr = null;

            List<CSMarker> markers = new List<CSMarker>();

            foreach (var item in allref)
            {
                if (curr != item.ReferencedObject)
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

                    curr = item.ReferencedObject;
                }

                if (!markers.Contains(item.CallingObject)) markers.Add(item.CallingObject);
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

    internal class MostSpreadOutNamespacesReport : ReportBase<ReportNode<IProjectNode>>
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