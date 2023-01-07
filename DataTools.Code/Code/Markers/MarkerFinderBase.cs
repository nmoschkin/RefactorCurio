using System;
using System.Collections.Generic;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// This is a base class for marker finders.
    /// </summary>
    internal abstract class MarkerFinderBase : IMarkerFinder
    {
        public abstract IMarker GetMarkerAtLine(int line);

        public abstract IMarker GetMarkerAt(int index);

        public virtual IMarker ScanMarker(IEnumerable<IMarker> markers, Func<IMarker, bool> scanFunc)
        {
            foreach (var marker in markers)
            {
                if (scanFunc(marker))
                {
                    return marker;
                }

                if (marker.Children != null)
                {
                    foreach (IMarker chm in marker.Children)
                    {
                        var res = ScanMarker(chm, scanFunc);
                        if (res != null) return res;
                    }
                }
            }

            return null;
        }

        public virtual IMarker ScanMarker(IMarker marker, Func<IMarker, bool> scanFunc)
        {
            if (scanFunc(marker))
            {
                return marker;
            }

            if (marker.Children != null)
            {
                foreach (IMarker chm in marker.Children)
                {
                    var res = ScanMarker(chm, scanFunc);
                    if (res != null) return res;
                }
            }

            return null;
        }

        public virtual T ScanMarker<T>(T marker, Func<IMarker, bool> scanFunc) where T : class, IMarker
        {
            if (scanFunc(marker))
            {
                return marker;
            }

            if (marker.Children != null)
            {
                foreach (IMarker chm in marker.Children)
                {
                    var res = ScanMarker(chm, scanFunc);
                    if (res != null && res is T tres) return tres;
                }
            }

            return null;
        }
    }
}