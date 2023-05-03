namespace DataTools.Code.Project
{
    /// <summary>
    /// Interface for an object that is at home inside of a namespace.
    /// </summary>
    public interface INamespace : IProjectNode
    {
        /// <summary>
        /// Home namespace of this element.
        /// </summary>
        ElementToken Namespace { get; set; }

        /// <summary>
        /// Gets the fully-qualified name calculated from the <see cref="Namespace"/> and <see cref="Name"/> properties.
        /// </summary>
        ElementToken FullyQualifiedName { get; }
    }
}