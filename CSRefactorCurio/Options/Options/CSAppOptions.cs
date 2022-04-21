using DataTools.CSTools;

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CSRefactorCurio.Options
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class CSAppOptionsOptions : BaseOptionPage<CSAppOptions> { }
    }

    public class CSAppOptions : BaseOptionModel<CSAppOptions>
    {
        [Category("JSON to C# Generation")]
        [DisplayName("Type to be used for floating point properties")]
        [Description("If the type is detected to be a floating point type, this will be the output type of the generated properties.")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(FPType.Double)]
        public FPType FloatNumberType { get; set; } = FPType.Double;

        [Category("JSON to C# Generation")]
        [DisplayName("Generate time conversion classes from templates if UNIX-like time conversions are detected.")]
        [Description("If this property is set to true, any long values from the example data that correspond to a UNIX-like (seconds, milliseconds, or nanoseconds) timestamp within 5 years of the current date will automatically be regarded as DateTime fields, and the required JsonConverter{T}-derived classes will be generated to decode these values. If this property is set to false, long values will be converted, as-is.")]
        [DefaultValue(true)]
        public bool GenerateTimeConverter { get; set; } = true;

        [Category("JSON to C# Generation")]
        [DisplayName("Type to be used for indeteriminate type")]
        [Description("If the numeric type cannot be detected, it will default to the specified mode to output the generated properties.")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(IndeterminateType.Float)]
        public IndeterminateType IndeterminateType { get; set; } = IndeterminateType.Float;

        [Category("JSON to C# Generation")]
        [DisplayName("Type to be used for integer properties")]
        [Description("If the type is detected to be an integral type, this will be the output type of the generated properties.")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(IntType.Long)]
        public IntType IntNumberType { get; set; } = IntType.Long;


        [Category("JSON to C# Generation")]
        [DisplayName("Generate MVVM Code")]
        [Description("Generate code for MVVM property setters that implement INotifyPropertyChanged using SetProperty(ref T, value) or custom template.")]
        [DefaultValue(false)]
        public bool MVVMSetProperty { get; set; } = false;


        [Category("JSON to C# Generation")]
        [DisplayName("Generate XML Document Markup")]
        [Description("If the JSON text is commented, then the comments will be incorporated as documentation markup. Otherwise, markup will be generated using the name of the element.")]
        [DefaultValue(false)]
        public bool GenerateDocStrings { get; set; } = true;
    }
}
