using System.ComponentModel;

namespace DataTools.Code.Project.Properties
{
    /// <summary>
    /// Simple interface for the EnvDTE properties.
    /// </summary>
    public interface IProperty : INotifyPropertyChanged
    {
        IPropertiesContainer Container { get; }

        string Name { get; }

        object Value { get; set; }
    }
}