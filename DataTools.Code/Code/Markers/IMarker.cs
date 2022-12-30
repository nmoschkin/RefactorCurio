using DataTools.Code.Project;

using System;
using System.Collections.Generic;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// Represents a logical segment in a source code file.
    /// </summary>
    internal interface IMarker : IProjectNode, INamespace, ICloneable
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
        /// The child elements.
        /// </summary>
        new IMarkerList Children { get; }

        /// <summary>
        /// The string content of this element block.
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// If applicable, gets the data type of the element.
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// The end column in the original document.
        /// </summary>
        int EndColumn { get; set; }

        /// <summary>
        /// The end line in the original document.
        /// </summary>
        int EndLine { get; set; }

        /// <summary>
        /// The last character position in the original document.
        /// </summary>
        int EndPos { get; set; }

        /// <summary>
        /// If applicable, the generic type parameters of this element.
        /// </summary>
        string Generics { get; set; }

        /// <summary>
        /// Gets the home source file of this marker.
        /// </summary>
        IProjectNode HomeFile { get; set; }

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
        /// The identified element marker kind.
        /// </summary>
        MarkerKind Kind { get; set; }

        /// <summary>
        /// The logical hierarchical level of this element in the original document.
        /// </summary>
        int Level { get; set; }

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
        /// The original parent element (should be a weak reference)
        /// </summary>
        IMarker ParentElement { get; set; }

        /// <summary>
        /// The name of the parent element.
        /// </summary>
        string ParentElementPath { get; set; }

        /// <summary>
        /// The scanned text that was used to determine the nature of the current element.
        /// </summary>
        string ScanHit { get; set; }

        /// <summary>
        /// The start column in the original document.
        /// </summary>
        int StartColumn { get; set; }

        /// <summary>
        /// The start line in the original document.
        /// </summary>
        int StartLine { get; set; }

        /// <summary>
        /// The first character position in the original document.
        /// </summary>
        int StartPos { get; set; }

        /// <summary>
        /// A list of words we did not understand. We'll examine these for type references.
        /// </summary>
        List<string> UnknownWords { get; set; }

        /// <summary>
        /// If applicable, the where clause of this element.
        /// </summary>
        string WhereClause { get; set; }

        /// <summary>
        /// Clone this marker into another object that implmenets <see cref="IMarker"/>.
        /// </summary>
        /// <param name="deep">Deeply copy the item by making a new collection for and copies of the children.</param>
        /// <typeparam name="T">The type of object to create, must be creatable.</typeparam>
        /// <returns>A new object based on this one.</returns>
        /// <remarks>Implementations should make note of when and where they cannot fulfill the <paramref name="deep"/> contract.</remarks>
        T Clone<T>(bool deep) where T : IMarker, new();

        /// <summary>
        /// Formats the contents of this marker for output to file.
        /// </summary>
        /// <returns></returns>
        string FormatContents();
    }

    /// <summary>
    /// Represents a strongly typed <see cref="IMarker"/> implementation with a strongly typed <see cref="IMarkerList{TMarker}"/> implementation for storing child markers.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal interface IMarker<TMarker, TList> : IMarker where TMarker : IMarker, new() where TList : IMarkerList<TMarker>, new()
    {
        /// <summary>
        /// The generated atomic source file information for this object.
        /// </summary>
        AtomicGenerationInfo<TMarker, TList> AtomicSourceFile { get; set; }

        /// <summary>
        /// The child elements.
        /// </summary>
        new TList Children { get; set; }
    }
}