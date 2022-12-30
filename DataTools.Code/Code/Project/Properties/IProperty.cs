using System.ComponentModel;

namespace DataTools.Code.Project.Properties
{
    /// <summary>
    /// Simple interface for the EnvDTE properties.
    /// </summary>
    internal interface IProperty : INotifyPropertyChanged
    {
        IPropertiesContainer Container { get; }

        string Name { get; }

        object Value { get; set; }
    }
}