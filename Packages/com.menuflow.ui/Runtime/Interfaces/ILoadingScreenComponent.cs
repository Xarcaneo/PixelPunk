using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MenuFlow.Interfaces
{
    /// <summary>
    /// Defines the contract for loading screen components that can be customized.
    /// This interface is the foundation of the loading screen system, allowing for
    /// different implementations of loading screen behaviors.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should:
    /// <list type="bullet">
    /// <item><description>Handle their own animation and state management</description></item>
    /// <item><description>Be attached to a GameObject with a CanvasGroup</description></item>
    /// <item><description>Properly clean up resources when hiding</description></item>
    /// </list>
    /// </remarks>
    public interface ILoadingScreenComponent
    {
        /// <summary>
        /// Shows the loading screen with any associated animations or effects.
        /// </summary>
        /// <returns>
        /// A Task that completes when the show animation is finished and the loading screen
        /// is fully visible and interactive.
        /// </returns>
        Task ShowAsync();

        /// <summary>
        /// Hides the loading screen with any associated animations or effects.
        /// </summary>
        /// <returns>
        /// A Task that completes when the hide animation is finished and the loading screen
        /// is fully hidden and non-interactive.
        /// </returns>
        Task HideAsync();

        /// <summary>
        /// Called when the scene load operation is about to begin.
        /// </summary>
        /// <param name="currentScene">The currently active scene</param>
        /// <param name="targetScene">The scene that will be loaded</param>
        /// <param name="loadMode">How the scene should be loaded</param>
        /// <returns>True if the component will handle scene activation, false to let the manager handle it</returns>
        Task<bool> OnBeforeSceneLoadAsync(Scene currentScene, string targetScene, LoadSceneMode loadMode);

        /// <summary>
        /// Called after the scene is loaded but before it's activated.
        /// </summary>
        /// <param name="loadedScene">The loaded but not yet activated scene</param>
        /// <returns>True when ready to activate the scene</returns>
        Task<bool> OnAfterSceneLoadAsync(Scene loadedScene);
    }
}
