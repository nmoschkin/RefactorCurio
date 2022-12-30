//using DataTools.Essentials.Observable;

//using System;
//using System.Collections;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;

//namespace DataTools.CSTools
//{
//    internal abstract class ProjectViewModelBase : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>
//    {
//        private ObservableCollection<IProjectElement> view;

//        private IProjectNode source;
//        public IProjectNode Source => source;

//        public ProjectViewModelBase(IProjectNode source)
//        {
//            this.source = source;
//        }

//        public abstract string Title { get; }

//        public ElementType ElementType => ElementType.ProjectView;

//        public ElementType ChildType => ElementType.Any;

//        public virtual ObservableCollection<IProjectElement> Children
//        {
//            get => view;
//            set
//            {
//                if (value != view)
//                {
//                    if (view != null)
//                    {
//                        view.CollectionChanged -= OnViewCollectionModified;
//                    }

//                    view = value;

//                    if (view != null)
//                    {
//                        view.CollectionChanged += OnViewCollectionModified;
//                    }

//                    SetupView();
//                    OnPropertyChanged();
//                }
//            }
//        }

//        IEnumerable IProjectNode.Children => view;

//        protected virtual void OnViewCollectionModified(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
//        {
//            SetupView();
//        }

//        protected abstract void SetupView();
//    }
//}