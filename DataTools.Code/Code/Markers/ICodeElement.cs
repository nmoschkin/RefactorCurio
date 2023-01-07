using System;
using System.Collections.Generic;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// An identifiable distinct element of code
    /// </summary>
    internal interface ICodeElement : ICloneable
    {
        /// <summary>
        /// If applicable, the <see cref="AccessModifiers"/> for the element.
        /// </summary>
        AccessModifiers AccessModifiers { get; set; }

        /// <summary>
        /// If applicable, the list of attributes.
        /// </summary>
        List<string> Attributes { get; set; }

        /// <summary>
        /// If applicable, gets the data type of the element.
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// If applicable, the generic type parameters of this element.
        /// </summary>
        string Generics { get; set; }

        /// <summary>
        /// If the <see cref="IsExtern"/> property, this sub-record will contain the DLL import information.
        /// </summary>
        IImportInfo ImportInfo { get; set; }

        /// <summary>
        /// If applicable, the inheritances of this element.
        /// </summary>
        List<string> Inheritances { get; set; }

        /// <summary>
        /// If applicable, the inheritance string of this element.
        /// </summary>
        string InheritanceString { get; set; }

        /// <summary>
        /// Element is marked abstract.
        /// </summary>
        bool IsAbstract { get; set; }

        /// <summary>
        /// Element is an async method.
        /// </summary>
        bool IsAsync { get; set; }

        /// <summary>
        /// Element is marked explicit
        /// </summary>
        bool IsExplicit { get; set; }

        /// <summary>
        /// Element is marked extern.
        /// </summary>
        bool IsExtern { get; set; }

        /// <summary>
        /// Element is marked implicit
        /// </summary>
        bool IsImplicit { get; set; }

        /// <summary>
        /// Element is marked new.
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Element is marked override.
        /// </summary>
        bool IsOverride { get; set; }

        /// <summary>
        /// Element is marked readonly.
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Element is a sealed class.
        /// </summary>
        bool IsSealed { get; set; }

        /// <summary>
        /// Element is a static member.
        /// </summary>
        bool IsStatic { get; set; }

        /// <summary>
        /// Element is marked virtual.
        /// </summary>
        bool IsVirtual { get; set; }

        /// <summary>
        /// If applicable, the list of method parameters.
        /// </summary>
        List<string> MethodParams { get; set; }

        /// <summary>
        /// If applicable, the method parameter string.
        /// </summary>
        string MethodParamsString { get; set; }

        /// <summary>
        /// The name of the element.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// If applicable, the where clause of this element.
        /// </summary>
        string WhereClause { get; set; }

        /// <summary>
        /// The identified element marker kind.
        /// </summary>
        MarkerKind Kind { get; set; }

        /// <summary>
        /// Clone this object (presumably deeply) into a new <see cref="ICodeElement"/> instance of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Clone<T>() where T : ICodeElement, new();
    }
}