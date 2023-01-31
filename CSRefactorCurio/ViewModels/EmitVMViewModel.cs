using CSRefactorCurio.Globalization.Resources;

using DataTools.Code.Markers;
using DataTools.CSTools;
using DataTools.Essentials.Converters.ClassDescriptions;
using DataTools.Essentials.Converters.ClassDescriptions.Framework;
using DataTools.Essentials.Converters.EnumDescriptions.Framework;
using DataTools.Essentials.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.ViewModels
{
    public enum EmitViewModelMode
    {
        Inherit,
        Source
    }

    /// <summary>
    /// Wrapper class for marker selection
    /// </summary>
    internal class BoolMarker : ObservableBase
    {
        private CSMarker marker;
        private bool selected;

        public BoolMarker(CSMarker marker)
        {
            this.marker = marker;
        }

        public string Title => marker.Name;

        public CSMarker Marker => marker;

        public bool IsSelected
        {
            get => selected;
            set
            {
                SetProperty(ref selected, value);
            }
        }
    }

    [Globalized(typeof(GenerationMode), typeof(AppResources))]
    public enum GenerationMode
    {
        [TranslationKey("MODE_INHERIT")]
        MODE_INHERIT,

        [TranslationKey("MODE_PLAIN")]
        MODE_PLAIN,

        [TranslationKey("MODE_PLAIN_INIT")]
        MODE_PLAIN_INIT,

        [TranslationKey("MODE_SOURCE")]
        MODE_SOURCE,
    }

    internal class EmitVMViewModel : ViewModelBase
    {
        private EmitViewModelMode mode;
        private ObservableCollection<BoolMarker> methods = new ObservableCollection<BoolMarker>();
        private ObservableCollection<BoolMarker> properties = new ObservableCollection<BoolMarker>();

        private string newFilename;

        private bool insertIntoCurrent;

        private CSMarker sourceClass;
        private string className;
        private bool cloneAvailable;

        private DescribedEnum<GenerationMode> selectedMode;

        private List<DescribedEnum<GenerationMode>> modes = new List<DescribedEnum<GenerationMode>>();

        public List<DescribedEnum<GenerationMode>> GenerationModes => modes;

        public DescribedEnum<GenerationMode> SelectedMode
        {
            get => selectedMode;
            set
            {
                SetProperty(ref selectedMode, value);
            }
        }

        public bool CloneAvailable
        {
            get => cloneAvailable;
            protected set
            {
                SetProperty(ref cloneAvailable, value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            methods.CollectionChanged -= OnMethodCollectionChanged;
            properties.CollectionChanged -= OnPropertyCollectionChanged;

            sync.Post((o) =>
            {
                methods.Clear();
                properties.Clear();
            }, null);

            base.Dispose(disposing);
        }

        public EmitVMViewModel(CSMarker sourceClass) : base(true, true, true, false)
        {
            if (sourceClass.Kind != MarkerKind.Class) throw new ArgumentException("Source Marker Must Be A Class");

            modes.Add(new DescribedEnum<GenerationMode>(GenerationMode.MODE_INHERIT));
            modes.Add(new DescribedEnum<GenerationMode>(GenerationMode.MODE_PLAIN));
            modes.Add(new DescribedEnum<GenerationMode>(GenerationMode.MODE_PLAIN_INIT));
            modes.Add(new DescribedEnum<GenerationMode>(GenerationMode.MODE_SOURCE));

            this.sourceClass = sourceClass;

            methods.CollectionChanged += OnMethodCollectionChanged;
            properties.CollectionChanged += OnPropertyCollectionChanged;

            className = sourceClass.Name + "ViewModel";

            if (sourceClass.HomeFile?.Filename is string s)
            {
                newFilename = Path.Combine(Path.GetDirectoryName(s), className + ".cs");
            }

            var mtd = sourceClass.Children.Where(c => c.Kind == MarkerKind.Method && c.AccessModifiers == DataTools.Code.AccessModifiers.Public).ToList();
            var prp = sourceClass.Children.Where(c => c.Kind == MarkerKind.Property && c.AccessModifiers == DataTools.Code.AccessModifiers.Public).ToList();

            prp = prp.Where(p => p.Children.Count(pc => pc.Name == "set") > 0).ToList();

            foreach (var m in mtd)
            {
                methods.Add(new BoolMarker(m));
            }

            foreach (var p in prp)
            {
                properties.Add(new BoolMarker(p));
            }

            OnPropertyChanged(nameof(Title));
            AutoRegisterCommands(this);
        }

        private void OnPropertyCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Changed = true;
        }

        private void OnMethodCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Changed = true;
        }

        public CSMarker SourceClass => sourceClass;

        public override string Title
        {
            get
            {
                var sb = new StringBuilder();
                if (Changed)
                {
                    sb.Append("* ");
                }

                sb.Append(AppResources.EMIT_VIEWMODEL);

                if (!string.IsNullOrEmpty(className))
                {
                    sb.Append($" ({className})");
                }

                return sb.ToString();
            }
        }

        public string ClassName
        {
            get => className;
            set
            {
                if (SetProperty(ref className, value))
                {
                    Changed = true;
                }
            }
        }

        public bool InsertIntoCurrent
        {
            get => insertIntoCurrent;
            set
            {
                if (SetProperty(ref insertIntoCurrent, value))
                {
                    Changed = true;
                }
            }
        }

        public string NewFilename
        {
            get => newFilename;
            set
            {
                if (SetProperty(ref newFilename, value))
                {
                    Changed = true;
                }
            }
        }

        public EmitViewModelMode Mode
        {
            get => mode;
            set
            {
                if (SetProperty(ref mode, value))
                {
                    Changed = true;
                }
            }
        }

        public ObservableCollection<BoolMarker> Methods
        {
            get => methods;
            set
            {
                if (SetProperty(ref methods, value))
                {
                    Changed = true;
                }
            }
        }

        public ObservableCollection<BoolMarker> Properties
        {
            get => properties;
            set
            {
                if (SetProperty(ref properties, value))
                {
                    Changed = true;
                }
            }
        }
    }
}