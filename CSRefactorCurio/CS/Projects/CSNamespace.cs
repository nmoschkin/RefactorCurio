using DataTools.CSTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{
    public class CSNamespace : ObservableBase, IProjectNode
    {
        private string name;
        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();

        private ObservableCollection<CSMarker> markers = new ObservableCollection<CSMarker>();

        private ObservableCollection<CSNamespace> namespaces = new ObservableCollection<CSNamespace>();

        private CSNamespace parent;
        
        
        public ObservableCollection<CSMarker> Markers
        {
            get => markers;
            protected set
            {
                SetProperty(ref markers, value);    
            }
        }

        public ObservableCollection<CSNamespace> Namespaces
        {
            get => namespaces;
            protected set
            {
                SetProperty(ref namespaces, value);
            }
        }

        public static ObservableCollection<IProjectElement> NamespacesFromProject(CurioProject project, Dictionary<string, CSNamespace> namespaces = null)
        {
            return NamespacesFromProjects(new[] { project }, namespaces);
        }
                
        public static ObservableCollection<IProjectElement> NamespacesFromProjects(IEnumerable<CurioProject> projects, Dictionary<string, CSNamespace> namespaces = null)
        {
            namespaces ??= new Dictionary<string, CSNamespace>();

            foreach (var project in projects)
            {
                var root = project.RootFolder;
                NamespacesFromNode(root, namespaces);
            }

            return new ObservableCollection<IProjectElement>(namespaces.Values.Where((v) => v.Parent == null));
        }

        private static CSNamespace EnsureNamespace(string name, Dictionary<string, CSNamespace> namespaces)
        {
            var ns = name.Split('.');
            int c = ns.Length;
            var sb = new StringBuilder();

            CSNamespace lvn = null;

            for (int x = 0; x < c; x++)
            {
                if (sb.Length > 0) sb.Append('.');
                sb.Append(ns[x]);

                if (!namespaces.TryGetValue(sb.ToString(), out CSNamespace vns))
                {
                    vns = new CSNamespace(sb.ToString(), lvn);
                    if (lvn != null)
                    {
                        lvn.Children.Add(vns);
                        lvn.Namespaces.Add(vns);
                    }

                    namespaces.Add(sb.ToString(), vns);
                }
        
                lvn = vns;
            }

            return lvn;
        }

        private static void NamespacesFromNode(CSDirectory node, Dictionary<string, CSNamespace> namespaces)
        {
            foreach (var file in node.Files) 
            {
                foreach (CSMarker cls in file.Children)
                {
                    var ns = EnsureNamespace(cls.Namespace, namespaces);

                    if (ns != null)
                    {
                        ns.Children.Add(cls);
                        ns.Markers.Add(cls);
                    }
                }
            }

            foreach (var dir in node.Directories)
            {
                NamespacesFromNode(dir, namespaces);
            }
        }

        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        public CSNamespace Parent => parent;

        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public CSNamespace(string name, CSNamespace parent = null)
        {
            this.name = name;
            this.parent = parent;
        }

        public CSNamespace(string name, IEnumerable<IProjectElement> items, CSNamespace parent = null) : this(name, parent)
        {
            foreach (var item in items)
            {
                children.Add(item);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public string Title => name;
        
        public ElementType ElementType => ElementType.Namespace;
    }
}
