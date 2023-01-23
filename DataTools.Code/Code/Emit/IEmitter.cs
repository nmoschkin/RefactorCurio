using DataTools.Code.Markers;

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DataTools.Code.Emit
{
    /// <summary>
    /// Implements a code emitter
    /// </summary>
    public interface IEmitter
    {
        /// <summary>
        /// Gets or sets the number of indentation spaces for formatted text.
        /// </summary>
        /// <remarks>
        /// 4 is the default value.
        /// </remarks>
        [DefaultValue(4)]
        int Indent { get; set; }

        /// <summary>
        /// Returns true if the emitter can handle the specified <see cref="MarkerKind"/>.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>True if the specified <see cref="MarkerKind"/> can be handled.</returns>
        bool CanEmit(MarkerKind kind);

        /// <summary>
        /// Emit the specified <see cref="IMarker"/> to the <see cref="StringBuilder"/> destination.
        /// </summary>
        /// <param name="marker">The marker to emit.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="level">The zero-based code indentation level.</param>
        /// <returns>True if text was written to the <see cref="StringBuilder"/>.</returns>
        bool Emit(IMarker marker, StringBuilder destination, int level);
    }

    public interface IEmitter<TMarker> : IEmitter where TMarker : IMarker, new()
    {
        /// <summary>
        /// Emit the specified <see cref="IMarker"/> to the <see cref="StringBuilder"/> destination.
        /// </summary>
        /// <param name="marker">The marker to emit.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="level">The zero-based code indentation level.</param>
        /// <returns>True if text was written to the <see cref="StringBuilder"/>.</returns>
        bool Emit(TMarker marker, StringBuilder destination, int level);
    }
}