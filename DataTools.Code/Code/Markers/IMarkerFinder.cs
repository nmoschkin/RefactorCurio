using System;
using System.Linq;
using System.Text;

namespace DataTools.Code.Markers
{
    internal interface IMarkerFinder
    {
        /// <summary>
        /// Return the first, deepest marker at the specific line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        IMarker GetMarkerAtLine(int line);

        /// <summary>
        /// Return the deepest marker at the specified character index in the parent file.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IMarker GetMarkerAt(int index);

        /// <summary>
        /// Find a marker somewhere in the specified marker and its descendants using a validation function.
        /// </summary>
        /// <param name="marker">The marker to scan along with its descendants.</param>
        /// <param name="scanFunc">The validation function used to evaluate a marker for match positivity.</param>
        /// <returns>The first marker that was evaluated as true by <paramref name="scanFunc"/>, or null if no match was found.</returns>
        IMarker ScanMarker(IMarker marker, Func<IMarker, bool> scanFunc);

        /// <summary>
        /// Find a marker of type <typeparamref name="T"/> somewhere in the specified marker and its descendants using a validation function.
        /// </summary>
        /// <param name="marker">The marker to scan along with its descendants.</param>
        /// <param name="scanFunc">The validation function used to evaluate a marker for match positivity.</param>
        /// <returns>The first marker of type <typeparamref name="T"/> that was evaluated as true by <paramref name="scanFunc"/>, or null if no match was found.</returns>
        T ScanMarker<T>(T marker, Func<IMarker, bool> scanFunc) where T : class, IMarker;
    }

    internal abstract class MarkerFinderBase : IMarkerFinder
    {
        public abstract IMarker GetMarkerAtLine(int line);

        public abstract IMarker GetMarkerAt(int index);

        public virtual IMarker ScanMarker(IMarker marker, Func<IMarker, bool> scanFunc)
        {
            if (scanFunc(marker))
            {
                if (marker.Children != null)
                {
                    foreach (IMarker chm in marker.Children)
                    {
                        var res = ScanMarker(chm, scanFunc);
                        if (res != null) return res;
                    }
                }

                return marker;
            }

            return null;
        }

        public virtual T ScanMarker<T>(T marker, Func<IMarker, bool> scanFunc) where T : class, IMarker
        {
            if (scanFunc(marker))
            {
                if (marker.Children != null)
                {
                    foreach (IMarker chm in marker.Children)
                    {
                        var res = ScanMarker(chm, scanFunc);
                        if (res != null && res is T tres) return tres;
                    }
                }

                return marker;
            }

            return null;
        }
    }
}