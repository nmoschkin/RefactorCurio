using DataTools.Code;
using DataTools.Code.Markers;
using DataTools.Code.Project;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataTools.CSTools
{
    /// <summary>
    /// CS Refactor Curio Solution Marker
    /// </summary>
    internal class CSMarker : MarkerBase<CSMarker, ObservableMarkerList<CSMarker>>, IProjectNode<ObservableMarkerList<CSMarker>>
    {
        public override CSMarker FindParent(MarkerKind parentKind)
        {
            var p = this.ParentElement as CSMarker;

            while (p != null)
            {
                if (p.Kind == parentKind) return p;
                p = p.ParentElement as CSMarker;
            }

            return null;
        }

        public override string FormatContents()
        {
            HomeFile.EnsureText();

            if (Children != null && Children.Count > 0)
            {
                var sb = new StringBuilder();

                var m = StartPos;
                var n = Children[0].StartPos;

                string s = "";

                if (Level > 0)
                {
                    s = new string(' ', (Level - 1) * 4);
                }

                sb.Append(s);
                sb.AppendLine(HomeFile.Text.Substring(m, n - m));

                foreach (var marker in Children)
                {
                    switch (marker.Kind)
                    {
                        case MarkerKind.LineComment:
                        case MarkerKind.BlockComment:
                        case MarkerKind.XMLDoc:
                            continue;

                        case MarkerKind.Directive:
                            if (marker.Content.StartsWith("#region") || marker.Content.StartsWith("#endregion"))
                            {
                                continue;
                            }
                            break;

                        default:
                            break;
                    }

                    sb.Append(marker.FormatContents());
                }

                sb.AppendLine();
                sb.Append(s);
                sb.AppendLine("}");

                return sb.ToString();
            }
            else
            {
                var fitext = HomeFile.Text.Substring(StartPos, EndPos - StartPos + 1);
                return fitext;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public new CSCodeFile HomeFile
        {
            get => (CSCodeFile)base.HomeFile;
            set => base.HomeFile = value;
        }

        public override ObservableMarkerList<CSMarker> Children
        {
            get => base.Children;
            set
            {
                if (base.Children != value)
                {
                    base.Children = value;
                    OnPropertyChanged();
                }
            }
        }

        public string[] ExtractAllUsings()
        {
            List<string> usings = new List<string>();

            if (kind == MarkerKind.Using)
            {
                if (!string.IsNullOrEmpty(InheritanceString))
                {
                    usings.Add(Name + " = " + InheritanceString);
                }
                else
                {
                    usings.Add(Name);
                }
            }

            foreach (var c in Children)
            {
                usings.AddRange(c.ExtractAllUsings());
            }

            return usings.ToArray();
        }

        public string[] ExtractAllGlobalUsings()
        {
            List<string> usings = new List<string>();

            if (kind == MarkerKind.Using && AccessModifiers == AccessModifiers.Global)
            {
                usings.Add(Name);
            }

            foreach (var c in Children)
            {
                usings.AddRange(c.ExtractAllUsings());
            }

            return usings.ToArray();
        }


        /// <summary>
        /// Gets all markers that represent a data type
        /// </summary>
        /// <typeparam name="T">The type of list to create</typeparam>
        /// <returns>A list of markers</returns>
        public virtual T GetAllTypes<T>() where T : class, IList<CSMarker>, new()
        {
            var tcol = new T();
            GetAllTypes(tcol);
            return tcol;
        }

        /// <summary>
        /// Gets all markers that represent a data type
        /// </summary>
        /// <param name="currList">The list of markers to populate</param>
        /// <returns>A list of markers</returns>
        public virtual void GetAllTypes<T>(T currList) where T : class, IList<CSMarker>, new()
        {
            var tcol = currList;

            if (KindIsType(Kind))
            {
                currList.Add(this);
            }

            if (Children != null && Children.Count > 0)
            {
                foreach (var item in Children)
                {
                    item.GetAllTypes(tcol);
                }
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}