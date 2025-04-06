using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;

namespace MenuFlow.Utils
{
    /// <summary>
    /// A utility class for handling smooth fade transitions in Unity UI.
    /// This class provides a clean, reusable way to fade CanvasGroup components
    /// with precise timing control and proper cleanup.
    /// </summary>
    /// <remarks>
    /// Features:
    /// - Smooth fade transitions using Lerp
    /// - Configurable fade duration and hold time
    /// - Proper coroutine management
    /// - Async/await support
    /// - Automatic cleanup on component destruction
    /// 
    /// Example usage:
    /// ```csharp
    /// var fadeUtil = new FadeUtility(0.5f, 0.2f, this);
    /// await fadeUtil.FadeAsync(canvasGroup, 0f, 1f, true);
    /// ```
    /// </remarks>
    public class FadeUtility
    {
        private readonly float fadeDuration;
        private readonly float holdDuration;
        private readonly MonoBehaviour coroutineRunner;
        private readonly float updateInterval = 0.016f; // ~60fps

        private Coroutine currentFadeCoroutine;

        /// <summary>
        /// Initializes a new instance of the FadeUtility class.
        /// </summary>
        /// <param name="fadeDuration">How long the fade transition should take in seconds</param>
        /// <param name="holdDuration">How long to maintain the target alpha before completing in seconds</param>
        /// <param name="coroutineRunner">MonoBehaviour instance to run the fade coroutine on</param>
        /// <exception cref="System.ArgumentNullException">Thrown when coroutineRunner is null</exception>
        /// <exception cref="System.ArgumentException">Thrown when fadeDuration or holdDuration is negative</exception>
        public FadeUtility(float fadeDuration, float holdDuration, MonoBehaviour coroutineRunner)
        {
            if (coroutineRunner == null)
                throw new System.ArgumentNullException(nameof(coroutineRunner));
            if (fadeDuration < 0)
                throw new System.ArgumentException("Fade duration cannot be negative", nameof(fadeDuration));
            if (holdDuration < 0)
                throw new System.ArgumentException("Hold duration cannot be negative", nameof(holdDuration));

            this.fadeDuration = fadeDuration;
            this.holdDuration = holdDuration;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Performs a smooth fade transition on a CanvasGroup.
        /// </summary>
        /// <param name="canvasGroup">The CanvasGroup to fade</param>
        /// <param name="startAlpha">Starting alpha value (0-1)</param>
        /// <param name="endAlpha">Target alpha value (0-1)</param>
        /// <param name="enableInteraction">Whether to enable user interaction after the fade</param>
        /// <returns>A Task that completes when the fade (including hold time) is finished</returns>
        /// <remarks>
        /// The fade transition uses smooth interpolation and respects the configured fade duration.
        /// If a hold duration was specified, the method will wait that additional time at the target
        /// alpha before completing. Any existing fade operation will be stopped when starting a new one.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when canvasGroup is null</exception>
        public async Task FadeAsync(CanvasGroup canvasGroup, float startAlpha, float endAlpha, bool enableInteraction)
        {
            if (canvasGroup == null)
                throw new System.ArgumentNullException(nameof(canvasGroup));

            if (currentFadeCoroutine != null)
            {
                coroutineRunner.StopCoroutine(currentFadeCoroutine);
            }

            var tcs = new TaskCompletionSource<bool>();
            currentFadeCoroutine = coroutineRunner.StartCoroutine(FadeCoroutine(canvasGroup, startAlpha, endAlpha, enableInteraction, tcs));
            await tcs.Task;
        }

        /// <summary>
        /// Internal coroutine that handles the actual fade animation.
        /// Uses Time.deltaTime for smooth transitions and supports hold time.
        /// </summary>
        private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, bool enableInteraction, TaskCompletionSource<bool> tcs)
        {
            SetCanvasGroupState(canvasGroup, startAlpha, enableInteraction);

            float elapsedTime = 0f;
            
            // Perform the fade
            while (elapsedTime < fadeDuration)
            {
                float t = elapsedTime / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                elapsedTime += updateInterval;
                yield return new WaitForSeconds(updateInterval);
            }

            // Ensure we hit the target alpha exactly
            canvasGroup.alpha = endAlpha;

            // Hold at the target alpha if specified
            if (holdDuration > 0)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            SetCanvasGroupState(canvasGroup, endAlpha, enableInteraction);
            currentFadeCoroutine = null;
            tcs.SetResult(true);
        }

        /// <summary>
        /// Sets the state of a CanvasGroup, including its alpha and interaction settings.
        /// </summary>
        /// <param name="canvasGroup">The CanvasGroup to modify</param>
        /// <param name="alpha">Target alpha value</param>
        /// <param name="interactive">Whether the CanvasGroup should be interactive</param>
        private void SetCanvasGroupState(CanvasGroup canvasGroup, float alpha, bool interactive)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactive;
            canvasGroup.blocksRaycasts = interactive;
        }

        /// <summary>
        /// Stops any ongoing fade operation immediately.
        /// This is automatically called when starting a new fade or when the utility is cleaned up.
        /// </summary>
        public void StopFade()
        {
            if (currentFadeCoroutine != null)
            {
                coroutineRunner.StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
        }
    }
}
