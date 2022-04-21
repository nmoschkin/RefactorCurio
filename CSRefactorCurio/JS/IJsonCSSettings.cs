using System.Collections.ObjectModel;

namespace DataTools.CSTools
{
    public interface IJsonCSSettings
    {
        /// <summary>
        /// Gets or sets what types will be used for floating point properties.
        /// </summary>
        FPType FloatNumberType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate time conversion classes if UNIX-like time conversions are detected.
        /// </summary>
        /// <remarks>
        /// If this property is set to true, any long values from the example data that correspond to a UNIX-like (seconds, milliseconds, or nanoseconds) timestamp within 5 years of the current date will automatically be regarded as <see cref="DateTime"/> fields, and the required <see cref="JsonConverter{T}"/>-derived classes will be generated to decode these values.<br/><br/>
        /// If this property is set to false, long values will be converted, as-is.
        /// </remarks>
        bool GenerateTimeConverter { get; set; }

        /// <summary>
        /// Gets or sets what kind of number to default to if the number type is potentially indeterminate.
        /// </summary>
        IndeterminateType IndeterminateType { get; set; }

        /// <summary>
        /// Gets or sets what type will be used for integer properties.
        /// </summary>
        IntType IntNumberType { get; set; }

        /// <summary>
        /// Generate XML Document Markup
        /// </summary>
        bool GenerateDocStrings { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates generating code for MVVM property setters using SetProperty(ref T, value).
        /// </summary>
        bool MVVMSetProperty { get; set; }

    }
}