using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Identify known and unknown logical elements.
    /// </summary>
    public enum MarkerKind
    {

        /// <summary>
        /// Unknown or Code
        /// </summary>
        [Description("Unknown")]
        Code = 0,

        /// <summary>
        /// Constructor
        /// </summary>
        [Description("Constructor")]
        Constructor = 0x101,

        /// <summary>
        /// Destructor
        /// </summary>
        [Description("Destructor")]
        Destructor = 0x102,

        /// <summary>
        /// Using
        /// </summary>
        [Description("Using")]
        Using = 3,

        /// <summary>
        /// Namespace
        /// </summary>
        [Description("Namespace")]
        Namespace = 0x4,

        /// <summary>
        /// this[] Indexer
        /// </summary>
        [Description("this[] Indexer")]
        This = 0x105,

        /// <summary>
        /// Class
        /// </summary>
        [Description("Class")]
        Class = 0x6,

        /// <summary>
        /// Interface
        /// </summary>
        [Description("Interface")]
        Interface = 0x7,

        /// <summary>
        /// Structure
        /// </summary>
        [Description("Structure")]
        Struct = 0x8,

        /// <summary>
        /// Enum Block
        /// </summary>
        [Description("Enum Block")]
        Enum = 0x9,

        /// <summary>
        /// Record
        /// </summary>
        [Description("Record")]
        Record = 0xa,

        /// <summary>
        /// Delegate
        /// </summary>
        [Description("Delegate")]
        Delegate = 0x10b,

        /// <summary>
        /// Event
        /// </summary>
        [Description("Event")]
        Event = 0x10c,

        /// <summary>
        /// Constant
        /// </summary>
        [Description("Constant")]
        Const = 0x10d,

        /// <summary>
        /// Operator
        /// </summary>
        [Description("Operator")]
        Operator = 0x10e,

        /// <summary>
        /// For Loop
        /// </summary>
        [Description("For Loop")]
        ForLoop = 0x20f,

        /// <summary>
        /// Do While
        /// </summary>
        [Description("Do While")]
        DoWhile = 0x210,

        /// <summary>
        /// While
        /// </summary>
        [Description("While")]
        While = 0x211,

        /// <summary>
        /// Switch
        /// </summary>
        [Description("Switch")]
        Switch = 0x212,

        /// <summary>
        /// Case
        /// </summary>
        [Description("Case")]
        Case = 0x213,

        /// <summary>
        /// Using Block
        /// </summary>
        [Description("Using Block")]
        UsingBlock = 0x214,

        /// <summary>
        /// Lock Block
        /// </summary>
        [Description("Lock Block")]
        Lock = 0x215,

        /// <summary>
        /// Unsafe Block
        /// </summary>
        [Description("Unsafe Block")]
        Unsafe = 0x216,

        /// <summary>
        /// Fixed Block
        /// </summary>
        [Description("Fixed Block")]
        Fixed = 0x217,

        /// <summary>
        /// For Each Loop
        /// </summary>
        [Description("For Each Loop")]
        ForEach = 0x218,

        /// <summary>
        /// Do Loop
        /// </summary>
        [Description("Do Loop")]
        Do = 0x219,

        /// <summary>
        /// Else
        /// </summary>
        [Description("Else")]
        Else = 0x21a,

        /// <summary>
        /// Else If
        /// </summary>
        [Description("Else If")]
        ElseIf = 0x21b,

        /// <summary>
        /// If
        /// </summary>
        [Description("If")]
        If = 0x21c,

        /// <summary>
        /// Property Get
        /// </summary>
        [Description("Property Get")]
        Get = 0x11d,

        /// <summary>
        /// Property Set
        /// </summary>
        [Description("Property Set")]
        Set = 0x11e,

        /// <summary>
        /// Event Handler Add
        /// </summary>
        [Description("Event Handler Add")]
        Add = 0x11f,

        /// <summary>
        /// Event Handler Remove
        /// </summary>
        [Description("Event Handler Remove")]
        Remove = 0x120,

        /// <summary>
        /// Field Value
        /// </summary>
        [Description("Field Value")]
        FieldValue = 0x221,

        /// <summary>
        /// Method
        /// </summary>
        [Description("Method")]
        Method = 0x122,

        /// <summary>
        /// Enum Value
        /// </summary>
        [Description("Enum Value")]
        EnumValue = 0x222,

        /// <summary>
        /// Property
        /// </summary>
        [Description("Property")]
        Property = 0x123,

        /// <summary>
        /// Field
        /// </summary>
        [Description("Field")]
        Field = 0x124,

        /// <summary>
        /// XML Documentation Markup
        /// </summary>
        [Description("XML Documentation Markup")]
        XMLDoc = 0x25,

        /// <summary>
        /// Line Comment
        /// </summary>
        [Description("Line Comment")]
        LineComment = 0x26,

        /// <summary>
        /// Block Comment
        /// </summary>
        [Description("Block Comment")]
        BlockComment = 0x27,

        /// <summary>
        /// Attribute or Decorator
        /// </summary>
        [Description("Attribute or Decorator")]
        Attribute = 0x28,

        /// <summary>
        /// Custom 1
        /// </summary>
        [Description("Custom 1")]
        Custom1 = 0x1001,

        /// <summary>
        /// Custom 2
        /// </summary>
        [Description("Custom 2")]
        Custom2 = 0x1002,

        /// <summary>
        /// Custom 3
        /// </summary>
        [Description("Custom 3")]
        Custom3 = 0x1003,

        /// <summary>
        /// Custom 4
        /// </summary>
        [Description("Custom 4")]
        Custom4 = 0x1004,

        Consolidation = 0x2000,

        /// <summary>
        /// Indicates a block level element beyond which we should not descend for listing purposes.
        /// </summary>
        IsBlockLevel = 0x100,

        /// <summary>
        /// Is a code-level element.
        /// </summary>
        IsCodeLevel = 0x200

    }

}