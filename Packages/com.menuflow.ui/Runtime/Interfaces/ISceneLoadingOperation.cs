using System;
using System.Threading.Tasks;

namespace MenuFlow.Interfaces
{
    /// <summary>
    /// Represents a scene loading operation.
    /// Abstracts the loading process to allow for different loading strategies.
    /// </summary>
    public interface ISceneLoadingOperation
    {
        /// <summary>
        /// Gets the name of the scene being loaded.
        /// </summary>
        string SceneName { get; }

        /// <summary>
        /// Gets whether the loading operation is complete.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Gets the current progress of the loading operation (0-1).
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Event triggered when the loading operation completes.
        /// </summary>
        event Action<ISceneLoadingOperation> OnLoadingComplete;

        /// <summary>
        /// Executes the loading operation asynchronously.
        /// </summary>
        Task ExecuteAsync();
    }
}
