using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Identify known and unknown logical elements.
    /// </summary>
    public enum MarkerKind
    {

        /// <summary>
        /// Unknown
        /// </summary>
        [Description("Unknown")]
        Unknown,

        /// <summary>
        /// Constructor
        /// </summary>
        [Description("Constructor")]
        Constructor,

        /// <summary>
        /// Destructor
        /// </summary>
        [Description("Destructor")]
        Destructor,

        /// <summary>
        /// Using
        /// </summary>
        [Description("Using")]
        Using,

        /// <summary>
        /// Namespace
        /// </summary>
        [Description("Namespace")]
        Namespace,

        /// <summary>
        /// this[] Indexer
        /// </summary>
        [Description("this[] Indexer")]
        This,

        /// <summary>
        /// Class
        /// </summary>
        [Description("Class")]
        Class,

        /// <summary>
        /// Interface
        /// </summary>
        [Description("Interface")]
        Interface,

        /// <summary>
        /// Structure
        /// </summary>
        [Description("Structure")]
        Struct,

        /// <summary>
        /// Enum Block
        /// </summary>
        [Description("Enum Block")]
        Enum,

        /// <summary>
        /// Record
        /// </summary>
        [Description("Record")]
        Record,

        /// <summary>
        /// Delegate
        /// </summary>
        [Description("Delegate")]
        Delegate,

        /// <summary>
        /// Event
        /// </summary>
        [Description("Event")]
        Event,

        /// <summary>
        /// Constant
        /// </summary>
        [Description("Constant")]
        Const,

        /// <summary>
        /// Operator
        /// </summary>
        [Description("Operator")]
        Operator,

        /// <summary>
        /// For Loop
        /// </summary>
        [Description("For Loop")]
        ForLoop,

        /// <summary>
        /// Do While
        /// </summary>
        [Description("Do While")]
        DoWhile,

        /// <summary>
        /// While
        /// </summary>
        [Description("While")]
        While,

        /// <summary>
        /// Switch
        /// </summary>
        [Description("Switch")]
        Switch,

        /// <summary>
        /// Case
        /// </summary>
        [Description("Case")]
        Case,

        /// <summary>
        /// Using Block
        /// </summary>
        [Description("Using Block")]
        UsingBlock,

        /// <summary>
        /// Lock Block
        /// </summary>
        [Description("Lock Block")]
        Lock,

        /// <summary>
        /// Unsafe Block
        /// </summary>
        [Description("Unsafe Block")]
        Unsafe,

        /// <summary>
        /// Fixed Block
        /// </summary>
        [Description("Fixed Block")]
        Fixed,

        /// <summary>
        /// For Each Loop
        /// </summary>
        [Description("For Each Loop")]
        ForEach,

        /// <summary>
        /// Do Loop
        /// </summary>
        [Description("Do Loop")]
        Do,

        /// <summary>
        /// Else
        /// </summary>
        [Description("Else")]
        Else,

        /// <summary>
        /// Else If
        /// </summary>
        [Description("Else If")]
        ElseIf,

        /// <summary>
        /// If
        /// </summary>
        [Description("If")]
        If,

        /// <summary>
        /// Property Get
        /// </summary>
        [Description("Property Get")]
        Get,

        /// <summary>
        /// Property Set
        /// </summary>
        [Description("Property Set")]
        Set,

        /// <summary>
        /// Event Handler Add
        /// </summary>
        [Description("Event Handler Add")]
        Add,

        /// <summary>
        /// Event Handler Remove
        /// </summary>
        [Description("Event Handler Remove")]
        Remove,

        /// <summary>
        /// Field Value
        /// </summary>
        [Description("Field Value")]
        FieldValue,

        /// <summary>
        /// Method
        /// </summary>
        [Description("Method")]
        Method,

        /// <summary>
        /// Enum Value
        /// </summary>
        [Description("Enum Value")]
        EnumValue,

        /// <summary>
        /// Property
        /// </summary>
        [Description("Property")]
        Property,

        /// <summary>
        /// Field
        /// </summary>
        [Description("Field")]
        Field,

        /// <summary>
        /// XML Documentation Markup
        /// </summary>
        [Description("XML Documentation Markup")]
        XMLDoc,

        /// <summary>
        /// Line Comment
        /// </summary>
        [Description("Line Comment")]
        LineComment,

        /// <summary>
        /// Block Comment
        /// </summary>
        [Description("Block Comment")]
        BlockComment,

        /// <summary>
        /// Custom 1
        /// </summary>
        [Description("Custom 1")]
        Custom1,

        /// <summary>
        /// Custom 2
        /// </summary>
        [Description("Custom 2")]
        Custom2,

        /// <summary>
        /// Custom 3
        /// </summary>
        [Description("Custom 3")]
        Custom3,

        /// <summary>
        /// Custom 4
        /// </summary>
        [Description("Custom 4")]
        Custom4
    }

}