﻿using System.Windows.Media;

namespace DataTools.Code.Project.Properties
{
    /// <summary>
    /// Simple interface for the colorable property.
    /// </summary>
    public interface IColorableProperty : IProperty
    {
        Color Background { get; }

        bool Bold { get; }

        Color Foreground { get; }
    }
}