using System;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// This is a base class for marker finders.
    /// </summary>
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