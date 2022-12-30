using DataTools.Code.Markers;

namespace DataTools.Code.Filtering
{
    /// <summary>
    /// A base class for providing a system for filtering <see cref="IMarker"/> items.
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/> item.</typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
    /// <typeparam name="TFilter">The type of <see cref="MarkerFilter{TMarker, TList}"/>.</typeparam>
    internal interface IMarkerFilterProvider<TMarker, TList, TFilter>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
        where TFilter : MarkerFilter<TMarker, TList>, new()
    {
        /// <summary>
        /// Gets the filter engine currently being used by the filter provider
        /// </summary>
        TFilter Filter { get; }

        /// <summary>
        /// Gets the list of items after the filters have been applied.
        /// </summary>
        TList FilteredItems { get; }

        /// <summary>
        /// Provides a filter based on the given element context.
        /// </summary>
        /// <param name="items">The list to provide the filter for.</param>
        /// <returns></returns>
        MarkerFilterRule ProvideFilterRule(TList items);

        /// <summary>
        /// Run the filter on the given list of items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>A new list of items that have been filtered.</returns>
        TList RunFilters(TList items);
    }

    /// <summary>
    /// A base class for providing a system for filtering <see cref="IMarker"/> items using the default <see cref="MarkerFilter{TMarker, TList}"/> engine..
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/> item.</typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TMarker}"/>.</typeparam>
    internal interface IMarkerFilterProvider<TMarker, TList> : IMarkerFilterProvider<TMarker, TList, MarkerFilter<TMarker, TList>>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
    }
}