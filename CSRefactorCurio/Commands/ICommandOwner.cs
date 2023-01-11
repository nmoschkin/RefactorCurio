using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio
{
    /// <summary>
    /// Represents an object that contains <see cref="IOwnedCommand"/> objects that can be invoked via user interaction.
    /// </summary>
    public interface ICommandOwner : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Request to execute the command specified by <paramref name="commandId"/>.
        /// </summary>
        /// <param name="commandId">The command to execute.</param>
        /// <returns>True if the command can be executed, otherwise false.</returns>
        bool RequestCanExecute(string commandId);

        /// <summary>
        /// Gets the <see cref="IOwnedCommand"/> object with the specified key or property name.
        /// </summary>
        /// <param name="key">The name of the command to retrieve.</param>
        /// <returns></returns>
        IOwnedCommand this[string key] { get; }

        /// <summary>
        /// Ask all commands to valid their executable state.
        /// </summary>
        void QueryAllCommands();
    }
}