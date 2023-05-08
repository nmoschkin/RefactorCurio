using DataTools.Code.Project;
using DataTools.CSTools;
using DataTools.Essentials.Observable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CSRefactorCurio.Services
{

    /// <summary>
    /// Disposable object to load the namespaces of the solution
    /// </summary>
    /// <remarks>
    /// Subscription to the status bar dispatcher is required. Use this object in a using block for best results.
    /// </remarks>
    internal class NamespaceLoader : StatusBarInformer<CSCodeFile>
    {
        public NamespaceLoader() : base(nameof(NamespaceLoader) + Guid.NewGuid(), true)
        {
        }

        /// <summary>
        /// Generate a namespace map from the specified projects.
        /// </summary>
        /// <param name="projects">The projects to generate the namespace map for.</param>
        /// <param name="namespaces">Optional namespace dictionary.</param>
        /// <returns>A new <see cref="ObservableCollection{T}"/> of <see cref="IProjectElement"/> objects.</returns>
        public ObservableCollection<IProjectElement> NamespacesFromProjects(IEnumerable<CurioProject> projects, Dictionary<string, CSNamespace> namespaces = null)
        {
            try
            {
                namespaces ??= new Dictionary<string, CSNamespace>();

                int tc = 0;

                foreach (var project in projects)
                {
                    var root = project.RootFolder;
                    tc += root.GetTotalFilesCount();
                }

                int proc = 0;

                SetCount(tc);

                foreach (var project in projects)
                {
                    var root = project.RootFolder;
                    NamespacesFromNode(root, namespaces, project.DefaultNamespace ?? project.AssemblyName ?? project.Title);
                }
            }
            finally
            {
                Inform(false);
            }

            return new ObservableCollection<IProjectElement>(namespaces.Values.Where((v) => v.ParentElement == null));
        }

        protected override string GetInformText(CSCodeFile data)
        {
            return data.Filename;
        }

        /// <summary>
        /// Get the namespaces for the specified node.
        /// </summary>
        /// <param name="node">The directory/node to map.</param>
        /// <param name="namespaces">The current namespace map.</param>
        private void NamespacesFromNode(CSDirectory node, Dictionary<string, CSNamespace> namespaces, string defaultNamespace)
        {
            Inform(node.Files, (file) =>
            {
                foreach (CSMarker cls in file.FilteredItems)
                {
                    var ns = EnsureNamespace(cls.Namespace, namespaces, defaultNamespace);

                    if (ns != null)
                    {
                        ns.Children.Add(cls);
                        ns.Markers.Add(cls);

                        ns.Sort();
                    }
                }
            }, false);

            foreach (var dir in node.Directories)
            {
                NamespacesFromNode(dir, namespaces, defaultNamespace);
            }            
        }

        /// <summary>
        /// Ensure that a dictionary map of namespaces has the specified namespace element, creating it if necessary.
        /// </summary>
        /// <param name="name">The fully qualified namespace.</param>
        /// <param name="namespaces">The namespace map.</param>
        /// <returns>The found/created namespace.</returns>
        private CSNamespace EnsureNamespace(string name, Dictionary<string, CSNamespace> namespaces, string defaultNamespace)
        {
            name ??= defaultNamespace;

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

                        lvn.Sort();
                    }

                    namespaces.Add(sb.ToString(), vns);
                }

                lvn = vns;
            }

            return lvn;
        }

    }
}
