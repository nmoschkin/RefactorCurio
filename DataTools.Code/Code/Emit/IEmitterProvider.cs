using DataTools.Code.Markers;
using DataTools.Code.Project;

namespace DataTools.Code.Emit
{
    /// <summary>
    /// Implements a method to provide <see cref="IEmitter"/> instances for the specified <see cref="MarkerKind"/>.
    /// </summary>
    public interface IEmitterProvider
    {
        /// <summary>
        /// Gets the context for the emitter provider.
        /// </summary>
        ISolutionElement Context { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        bool CanProvideEmitter(MarkerKind kind);

        /// <summary>
        /// Provide an emitter for the specified marker.
        /// </summary>
        /// <param name="marker">The marker to get an emitter for.</param>
        /// <returns></returns>
        IEmitter ProvideEmitter(IMarker marker);
    }

    public interface IEmitterProvider<TMarker> : IEmitterProvider
        where TMarker : IMarker, new()
    {
        /// <summary>
        /// Provide an emitter for the specified marker.
        /// </summary>
        /// <param name="marker">The marker to get an emitter for.</param>
        /// <returns></returns>
        IEmitter<TMarker> ProvideEmitter(TMarker marker);
    }
}