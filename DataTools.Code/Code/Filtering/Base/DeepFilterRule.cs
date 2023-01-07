using DataTools.Code.Markers;

using System.Collections.Generic;
using System.Linq;

namespace DataTools.Code.Filtering.Base
{
    /// <summary>
    /// A filter rule that scans for and clones positive results, recursively.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    /// <remarks>
    /// If a match is found 7 layers deep, then all 7 layers are cloned, <br/>
    /// but only enough items are cloned to complete the hierarchy down to the positive matches.
    /// <br /><br />
    /// The return result will be a <typeparamref name="TList"/> of clones of all objects that are valid or contain valid children.<br />
    /// The new, cloned <typeparamref name="TMarker"/> objects will contain only valid children, or children that contain valid children.
    /// </remarks>
    public abstract class DeepFilterRule<TMarker, TList> : MarkerFilterRule<TMarker, TList>
        where TMarker : IMarker<TMarker, TList>, new()
        where TList : IMarkerList<TMarker>, new()
    {
        /// <summary>
        /// Gets the XML before a match item, so that the XML documentation can be cloned with the match.
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected IList<TMarker> GetXMLBefore(TMarker marker, TList list)
        {
            int i, c = list.Count;

            for (i = 0; i < c; i++)
            {
                if (list[i].Equals(marker))
                {
                    if (i > 0)
                    {
                        i--;
                        var l = new List<TMarker>();

                        while (i >= 0 && list[i].Kind == MarkerKind.XMLDoc)
                        {
                            l.Add(list[i]);
                            i--;
                        }

                        if (l.Count > 0)
                        {
                            l.Reverse();
                            return l;
                        }

                        break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Apply the filter to the items and all sub-items by calling <see cref="MarkerFilterRule.IsValid(IMarker)"/><br />
        /// on each item, and cloning each positive result.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>
        /// A <typeparamref name="TList"/> of clones of all objects that are valid or contain valid children.<br />
        /// The new, cloned <typeparamref name="TMarker" /> objects will contain only valid children, or children that contain valid children.
        /// </returns>
        /// <remarks>
        /// If a match is found 7 layers deep, then all 7 layers are cloned, but only<br/>
        /// enough items are cloned to complete the hierarchy down to the positive matches.<br /><br />
        /// </remarks>
        public override sealed TList ApplyFilter(TList items)
        {
            var l = new List<TMarker>();

            foreach (var item in items)
            {
                var bValid = IsValid(item);
                if (bValid || (item?.Children.Count ?? 0) > 0)
                {
                    var newitem = item.Clone<TMarker>(false);
                    newitem.ParentElement = null;

                    if (item.Children is TList iclist)
                    {
                        var cl = ApplyFilter(iclist);
                        if (cl.Count > 0)
                        {
                            newitem.Children = new TList();
                            foreach (var icl in cl)
                            {
                                icl.ParentElement = newitem;
                                newitem.Children.Add(icl);
                            }
                        }
                    }
                    if (bValid || (newitem?.Children.Count ?? 0) > 0)
                    {
                        var xmll = GetXMLBefore(item, items);
                        if (xmll != null)
                        {
                            foreach (var xf in xmll)
                            {
                                var xfi = xf.Clone<TMarker>(false);

                                xfi.ParentElement = null;
                                xfi.Children = default;

                                l.Add(xfi);
                            }
                        }
                        l.Add(newitem);
                    }
                }
            }

            if (l.Count == 0) return new TList();

            // our final list will be all elements who have no TMarker parents
            var e = l.Select(x =>
            {
                if (x.ParentElement is TMarker zpn)
                {
                    IMarker zp = x.ParentElement;
                    while (zp is TMarker zpa)
                    {
                        zpn = zpa;
                        zp = zp.ParentElement;
                    }

                    return zpn;
                }
                else
                {
                    return x;
                }
            }).Distinct().ToList();

            var u = new TList();

            foreach (var item in e)
            {
                u.Add(item);
            }

            return u;
        }
    }
}