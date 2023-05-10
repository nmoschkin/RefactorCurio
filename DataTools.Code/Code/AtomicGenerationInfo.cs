using DataTools.Code.Markers;

using System.Collections.Generic;

namespace DataTools.Code
{
    /// <summary>
    /// Represents a rendered and reformatted source code file containing the preamble and only the specified item from the original code.
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/> that will be used.</typeparam>
    /// <typeparam name="TList">The type of list that will contain the markers.</typeparam>
    public class AtomicGenerationInfo<TMarker, TList> where TMarker : IMarker, new() where TList : IList<TMarker>, new()
    {
        /// <summary>
        /// The source code document lines.
        /// </summary>
        public FileLines Lines { get; set; }

        /// <summary>
        /// The <see cref="IMarker"/> collection.
        /// </summary>
        public TList Markers { get; set; }

        /// <summary>
        /// Character position in the source file where the preamble begins.
        /// </summary>
        public int PreambleBegin { get; set; }

        /// <summary>
        /// Character position in the source file where the preamble ends.
        /// </summary>
        public int PreambleEnd { get; set; }

        public override string ToString()
        {
            return Lines?.ToString() ?? base.ToString();
        }
    }
}