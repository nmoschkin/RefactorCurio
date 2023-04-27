namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents the basis for all solution elements.
    /// </summary>
    public interface ISolutionElement 
    {
        /// <summary>
        /// The element type.
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// The title of the element.
        /// </summary>
        string Title { get; }
    }
}