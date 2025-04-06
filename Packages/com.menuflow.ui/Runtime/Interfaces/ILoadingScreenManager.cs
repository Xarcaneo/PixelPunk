using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MenuFlow.Interfaces
{
    /// <summary>
    /// Defines the contract for managing loading screen operations.
    /// Follows Single Responsibility Principle by focusing solely on loading screen management.
    /// </summary>
    public interface ILoadingScreenManager
    {
        /// <summary>
        /// Shows the loading screen and manages the scene transition.
        /// </summary>
        /// <param name="targetSceneName">Name of the scene to load</param>
        /// <param name="loadMode">How the scene should be loaded</param>
        /// <returns>Task representing the loading operation</returns>
        Task ShowLoadingScreenAsync(string targetSceneName, LoadSceneMode loadMode = LoadSceneMode.Single);

        /// <summary>
        /// Hides the current loading screen if one is active.
        /// </summary>
        /// <returns>Task representing the unload operation</returns>
        Task HideLoadingScreenAsync();

        /// <summary>
        /// Checks if a loading screen is currently active.
        /// </summary>
        bool IsLoadingScreenActive { get; }
    }
}
