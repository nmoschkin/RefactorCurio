using DataTools.Code.Markers;

namespace DataTools.Code.Filtering
{
    /// <summary>
    /// Interface for an object that works with filter rules to filter <see cref="IMarker"/> items.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/> to filter.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarker{TMarker, TList}"/>.</typeparam>
    internal class MarkerFilter<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        /// <summary>
        /// Apply the given rule, return a new list of items based on the specified rule.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <param name="rule">The rule to apply.</param>
        /// <param name="recursiveForSimpleRules">True to run rules based on <see cref="MarkerFilterRule"/> recursively.</param>
        /// <returns>The filtered list.</returns>
        /// <remarks>
        /// Simple rules are run recursively as specified by the <paramref name="recursiveForSimpleRules"/> parameter.<br /><br />
        /// Rules based on <see cref="MarkerFilterRule{TMarker, TList}"/> must define their own recursion tactics.
        /// </remarks>
        public virtual TList ApplyFilter(TList items, MarkerFilterRule rule, bool recursiveForSimpleRules = true)
        {
            // strong rules have their own filtering.
            if (rule is MarkerFilterRule<TMarker, TList> strongRule)
            {
                return ApplyFilter(items, strongRule);
            }

            var newList = new TList();

            foreach (var item in items)
            {
                if (rule.IsValid(item))
                {
                    // we want a shallow copy, here.  The filter recursion will create a deeper copy for children.
                    var newItem = (TMarker)item.Clone();

                    newList.Add(newItem);
                    ApplyFilter(newItem.Children, rule);
                }
            }

            return newList;
        }

        /// <summary>
        /// Apply the given rule by calling the <see cref="MarkerFilterRule{TMarker, TList}.ApplyFilter(TList)"/> method.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public virtual TList ApplyFilter(TList items, MarkerFilterRule<TMarker, TList> rule)
        {
            return rule.ApplyFilter(items);
        }
    }
}