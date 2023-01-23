using DataTools.Code.Markers;

using System;
using System.Linq;
using System.Text;

namespace DataTools.Code.CS.Emit
{
    internal abstract class CSEmitterBase<TMarker, TList>
        where TMarker : IMarker<TMarker, TList>, new()
        where TList : class, IMarkerList<TMarker>, new()
    {
    }
}