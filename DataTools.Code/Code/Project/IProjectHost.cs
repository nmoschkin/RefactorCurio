﻿using DataTools.Code.Project.Properties;

using System.ComponentModel;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents a top-level project host element.
    /// </summary>
    public interface IProjectHost : IProjectElement, INotifyPropertyChanged
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