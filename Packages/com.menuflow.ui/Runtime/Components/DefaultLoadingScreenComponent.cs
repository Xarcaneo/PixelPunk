using System.Threading.Tasks;
using UnityEngine;

namespace MenuFlow.Components
{
    /// <summary>
    /// A basic implementation of a loading screen that instantly shows/hides without animations.
    /// This component is useful for simple loading screens or as a starting point for custom implementations.
    /// </summary>
    /// <remarks>
    /// Features:
    /// <list type="bullet">
    /// <item><description>Instant visibility changes without animations</description></item>
    /// <item><description>Minimal overhead and complexity</description></item>
    /// <item><description>Ideal for prototyping or simple loading screens</description></item>
    /// </list>
    /// </remarks>
    [AddComponentMenu("MenuFlow/Default Loading Screen")]
    public class DefaultLoadingScreenComponent : BaseLoadingScreenComponent
    {
        /// <summary>
        /// Shows the loading screen instantly without animation.
        /// </summary>
        /// <returns>
        /// A Task that completes immediately after the loading screen is shown.
        /// </returns>
        public override async Task ShowAsync()
        {
            SetCanvasGroupState(1f, true);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Hides the loading screen instantly without animation.
        /// </summary>
        /// <returns>
        /// A Task that completes immediately after the loading screen is hidden.
        /// </returns>
        public override async Task HideAsync()
        {
            SetCanvasGroupState(0f, false);
            await Task.CompletedTask;
        }
    }
}
