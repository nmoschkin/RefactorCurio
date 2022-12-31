using DataTools.Essentials.Observable;

using System;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.ViewModels
{
    internal enum ShowRefMode
    {
        Any,
        Has,
        HasNot
    }

    internal class ExplorerFilterViewModel : ObservableBase
    {
        private bool showpublics;
        private bool showinternals;

        private ShowRefMode refmode;

        public bool RefModeAny
        {
            get => refmode == ShowRefMode.Any;
            set
            {
                if (value)
                {
                    refmode = ShowRefMode.Any;
                }

                OnPropertyChanged();
            }
        }

        public bool RefModeHas
        {
            get => refmode == ShowRefMode.Has;
            set
            {
                if (value)
                {
                    refmode = ShowRefMode.Has;
                }

                OnPropertyChanged();
            }
        }

        public bool RefModeHasNot
        {
            get => refmode == ShowRefMode.HasNot;
            set
            {
                if (value)
                {
                    refmode = ShowRefMode.HasNot;
                }

                OnPropertyChanged();
            }
        }

        public bool ShowPublics
        {
            get => showpublics;
            set
            {
                SetProperty(ref showpublics, value);
            }
        }

        public bool ShowInternals
        {
            get => showinternals;
            set
            {
                SetProperty(ref showinternals, value);
            }
        }

        public ShowRefMode RefMode
        {
            get => refmode;
            set
            {
                SetProperty(ref refmode, value);
            }
        }
    }
}