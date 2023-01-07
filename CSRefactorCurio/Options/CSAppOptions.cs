using DataTools.Code.JS;

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CSRefactorCurio.Options
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class CSAppOptionsOptions : BaseOptionPage<CSAppOptions>
        { }

        [ComVisible(true)]
        public class FilterOptionsOptions : BaseOptionPage<FilterOptions>
        { }
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
        [DisplayName("Type to be used for indeterminate type")]
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

        [Category("Project Cleanup")]
        [DisplayName("Use Separate Folders For Object Types")]
        [Description("Create the folders based on default folder name options for the given objects.")]
        [DefaultValue(false)]
        public bool UseSeparateFolders { get; set; } = false;

        [Category("Project Cleanup")]
        [DisplayName("Class Folder Name")]
        [Description("The name of the folder to put classes. Blank to leave in the same folder.")]
        [DefaultValue("")]
        public string ClassFolderName { get; set; } = "";

        [Category("Project Cleanup")]
        [DisplayName("Interface Folder Name")]
        [Description("The name of the folder to put Interfaces. Blank to leave in the same folder.")]
        [DefaultValue("Contracts")]
        public string InterfaceFolderName { get; set; } = "Contracts";

        [Category("Project Cleanup")]
        [DisplayName("Struct Folder Name")]
        [Description("The name of the folder to put Structs. Blank to leave in the same folder.")]
        [DefaultValue("Structs")]
        public string StructFolderName { get; set; } = "Structs";

        [Category("Project Cleanup")]
        [DisplayName("Enum Folder Name")]
        [Description("The name of the folder to put Enums. Blank to leave in the same folder.")]
        [DefaultValue("Enums")]
        public string EnumFolderName { get; set; } = "Enums";

        [Category("Project Cleanup")]
        [DisplayName("Delegate Folder Name")]
        [Description("The name of the folder to put Delegates. Blank to leave in the same folder.")]
        [DefaultValue("")]
        public string DelegateFolderName { get; set; } = "";
    }
}