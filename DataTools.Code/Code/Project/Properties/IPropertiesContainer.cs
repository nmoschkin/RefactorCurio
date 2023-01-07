using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataTools.Code.Project.Properties
{
    /// <summary>
    /// Simple interface for a property container.
    /// </summary>
    public interface IPropertiesContainer
    {
        object Parent { get; }
    }

    /// <summary>
    /// Simple interface for a property container.
    /// </summary>
    public interface IPropertiesContainer<T> : IPropertiesContainer, INotifyPropertyChanged, IEnumerable<T> where T : IProperty
    {
        T this[string name] { get; }
    }
}