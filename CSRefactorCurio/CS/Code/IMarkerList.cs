using System.Collections;
using System.Collections.Generic;

namespace DataTools.CSTools
{
    /// <summary>
    /// Base interface for marker lists. Inherits from <see cref="IEnumerable"/>.
    /// </summary>
    internal interface IMarkerList : IEnumerable
    {
    }

    /// <summary>
    /// Base interface for strongly-typed marker lists.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/></typeparam>
    internal interface IMarkerList<TMarker> : IMarkerList, IList<TMarker> where TMarker : IMarker
    {
    }
}