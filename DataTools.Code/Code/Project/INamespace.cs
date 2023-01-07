namespace DataTools.Code.Project
{
    public interface INamespace : IProjectNode
    {
        /// <summary>
        /// Home namespace of this element.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// Gets the fully-qualified name calculated from the <see cref="Namespace"/> and <see cref="Name"/> properties.
        /// </summary>
        string FullyQualifiedName { get; }
    }
}