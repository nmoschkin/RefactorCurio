﻿using DataTools.Code.Project;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataTools.Code.Reporting
{
    public interface IReportNode : INotifyPropertyChanged, IProjectElement
    {
        object Element { get; }

        IList AssociatedList { get; }
    }

    public interface IReportNode<T> : IReportNode, INotifyPropertyChanged
    {
        new T Element { get; }

        new IList<T> AssociatedList { get; }
    }
}