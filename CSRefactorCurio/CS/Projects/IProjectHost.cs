using CSRefactorCurio;

using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a top-level project host element.
    /// </summary>
    internal interface IProjectHost : IProjectElement, INotifyPropertyChanged
    {
        /// <summary>
        /// The project properties.
        /// </summary>
        IPropertiesContainer Properties { get; }

        /// <summary>
        /// The root folder of the project children.
        /// </summary>
        IProjectNode RootFolder { get; }
    }
}