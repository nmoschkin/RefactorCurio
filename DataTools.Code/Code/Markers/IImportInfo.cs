using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// Interface for a class that contains extern import information
    /// </summary>
    public interface IImportInfo : ICloneable, IEquatable<IImportInfo>
    {

        /// <summary>
        /// The name of the DLL
        /// </summary>
        string Library { get; set; }

        /// <summary>
        /// Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.
        /// </summary>
        bool BestFitMapping { get; set; }

        /// <summary>
        /// Indicates the calling convention of an entry point.
        /// </summary>
        CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Indicates how to marshal string parameters to the method and controls name mangling.
        /// </summary>
        CharSet CharSet { get; set; }

        /// <summary>
        /// Indicates the name or ordinal of the DLL entry point to be called.
        /// </summary>
        string EntryPoint { get; set; }

        /// <summary>
        /// Controls whether the CharSet field causes the common language runtime to search an unmanaged DLL for entry-point names other than the one specified.
        /// </summary>
        bool ExactSpelling { get; set; }

        /// <summary>
        /// Indicates whether unmanaged methods that have HRESULT return values are directly translated or whether HRESULT return values are automatically converted to exceptions.
        /// </summary>
        bool PreserveSig { get; set; }

        /// <summary>
        /// Indicates whether the callee sets an error (SetLastError on Windows or errno on other platforms) before returning from the attributed method.
        /// </summary>
        bool SetLastError { get; set; }

        /// <summary>
        /// Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI "?" character.
        /// </summary>
        bool ThrowOnUnmappableChar { get; set; }
    }
}