using DataTools.Code.Filtering;
using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

using System.Linq;

namespace DataTools.Code.CS.Filtering
{
    /// <summary>
    /// A filter rule to implement code filter blocks.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal class CodeFilterRule
        <TMarker, TList> : DeepFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        private CodeFilterOptions options;

        private FilterPassMode passMode;

        public FilterPassMode PassMode
        {
            get => passMode;
            set => passMode = value;
        }

        /// <summary>
        /// Create a new code filter rule with empty <see cref="Options"/>.
        /// </summary>
        public CodeFilterRule() : base()
        {
            options = new CodeFilterOptions();
        }

        /// <summary>
        /// Create a new code filter rule with the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options used to populate the <see cref="Options"/> property of the newly created instance.</param>
        /// <remarks>
        /// The <see cref="CodeFilterOptions"/> property is cloned. A direct object reference to the original is not retained.
        /// </remarks>
        public CodeFilterRule(CodeFilterOptions options) : base()
        {
            this.options = options.Clone();
        }

        /// <summary>
        /// Create a new code filter rule and populate its <see cref="Options"/> property using the specified <see cref="ICodeElement"/> object as a template.
        /// </summary>
        /// <param name="optionsTemplate">The code element template.</param>
        /// <param name="falseIsNull">If true, boolean items that are false in the <paramref name="optionsTemplate"/> are set to null in the newly created <see cref="Options"/> object.</param>
        /// <remarks>
        /// If <paramref name="falseIsNull"/> is true, boolean items that are false in the <paramref name="optionsTemplate"/> are set to null in the newly created <see cref="Options"/> object.
        /// </remarks>
        public CodeFilterRule(ICodeElement optionsTemplate, bool falseIsNull = true) : this()
        {
            Options = new CodeFilterOptions(optionsTemplate, falseIsNull);
        }

        /// <summary>
        /// Gets or sets the currently active code filtering options
        /// </summary>
        /// <remarks>
        /// If this property is set to null, this has the effect of clearing the options, but the object is never null.
        /// </remarks>
        public CodeFilterOptions Options
        {
            get => options;
            set
            {
                if (value != options && value != null)
                {
                    options = value.Clone();
                }
                else if (value == null)
                {
                    options = new CodeFilterOptions();
                }
            }
        }

        public override bool IsValid(IMarker item)
        {
            return Options.Validate(item);
        }
    }
}