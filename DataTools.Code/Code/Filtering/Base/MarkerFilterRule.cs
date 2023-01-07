using DataTools.Code.Markers;

using System.Linq;

namespace DataTools.Code.Filtering.Base
{
    /// <summary>
    /// Represents the base for marker filter rules.
    /// </summary>
    internal abstract class MarkerFilterRule
    {
        /// <summary>
        /// Determines whether the specified item is valid for the filter rule.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes the filter, otherwise false.</returns>
        public abstract bool IsValid(IMarker item);
    }

    /// <summary>
    /// Base class for a strongly-typed, self-applying filter rule.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/> element type.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarkerList{TMarker}"/> type.</typeparam>
    internal abstract class MarkerFilterRule<TMarker, TList> : MarkerFilterRule
        where TMarker : IMarker, new()
        where TList : IMarkerList<TMarker>, new()
    {
        /// <summary>
        /// Filter the items based on the rule rule.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <returns>A filtered list of items.</returns>
        public abstract TList ApplyFilter(TList items);
    }
}