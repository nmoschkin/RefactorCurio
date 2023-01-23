using DataTools.Code.Markers;

using System.Text;

namespace DataTools.Code.Emit
{
    internal abstract class EmitterBase : IEmitter
    {
        public virtual int Indent { get; set; } = 4;

        public abstract bool CanEmit(MarkerKind kind);

        public abstract bool Emit(IMarker marker, StringBuilder destination, int level);
    }

    internal abstract class Emitter<TMarker> : EmitterBase, IEmitter<TMarker>
        where TMarker : IMarker, new()
    {
        public override sealed bool Emit(IMarker marker, StringBuilder destination, int level)
        {
            return Emit((TMarker)marker, destination, level);
        }

        public abstract bool Emit(TMarker marker, StringBuilder destination, int level);
    }
}