using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataTools.Code.Markers
{
    public interface IImportInfo : ICloneable, IEquatable<IImportInfo>
    {
        string Library { get; set; }

        bool BestFitMapping { get; set; }
        // Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.

        CallingConvention CallingConvention { get; set; }
        // Indicates the calling convention of an entry point.

        CharSet CharSet { get; set; }
        // Indicates how to marshal string parameters to the method and controls name mangling.

        string EntryPoint { get; set; }
        // Indicates the name or ordinal of the DLL entry point to be called.

        bool ExactSpelling { get; set; }
        // Controls whether the CharSet field causes the common language runtime to search an unmanaged DLL for entry-point names other than the one specified.

        bool PreserveSig { get; set; }
        // Indicates whether unmanaged methods that have HRESULT return values are directly translated or whether HRESULT return values are automatically converted to exceptions.

        bool SetLastError { get; set; }
        // Indicates whether the callee sets an error (SetLastError on Windows or errno on other platforms) before returning from the attributed method.

        bool ThrowOnUnmappableChar { get; set; }
        // Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI "?" character.
    }
}