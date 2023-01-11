using System.Windows.Input;

namespace CSRefactorCurio
{
    /// <summary>
    /// Represents a command that can be invoked by user interaction that is owned by a <see cref="ICommandOwner"/> object.
    /// </summary>
    public interface IOwnedCommand : ICommand, IDisposable
    {
        /// <summary>
        /// Gets the unique ID for this command
        /// </summary>
        string CommandId { get; }

        /// <summary>
        /// The owner of the owned command
        /// </summary>
        ICommandOwner Owner { get; }

        /// <summary>
        /// Tri-state is-enabled.
        /// </summary>
        bool? IsEnabled { get; set; }

        /// <summary>
        /// Query whether this command can be executed
        /// </summary>
        /// <returns></returns>
        bool QueryCanExecute();
    }
}