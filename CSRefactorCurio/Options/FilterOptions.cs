using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Options
{
    internal class FilterOptions : BaseOptionModel<FilterOptions>
    {
        [Category("Filter")]
        [DisplayName("Configure Profile")]
        [Description("Select a profile or create a new filter profile to configure. This profile will appear in the Curio Explorer.")]
        public Dictionary<string, FilterProfile> Profiles { get; set; }
    }
}