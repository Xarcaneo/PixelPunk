using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.SceneManagement;
using MenuFlow.Utils;

namespace MenuFlow.Components
{
    /// <summary>
    /// A loading screen component that provides smooth fade transitions during scene loading.
    /// This component handles the visual transition between scenes using a fade effect and manages
    /// scene loading states. It requires a CanvasGroup and Image component to function properly.
    /// </summary>
    /// <remarks>
    /// Key features:
    /// - Configurable fade in/out duration and hold time
    /// - Smooth alpha transitions using Unity's CanvasGroup
    /// - Scene loading progress tracking
    /// - Automatic scene activation management
    /// </remarks>
    [AddComponentMenu("MenuFlow/Fade Loading Screen")]
    public class FadeLoadingScreenComponent : BaseLoadingScreenComponent
    {
        /// <summary>
        /// How long the fade transition takes to complete in seconds.
        /// This affects both fade in and fade out animations.
        /// </summary>
        [Tooltip("Duration of the fade animation in seconds")]
        [SerializeField] private float fadeDuration = 0.5f;

        /// <summary>
        /// Time to maintain full opacity before starting to fade out.
        /// Useful for ensuring the loading screen is visible long enough to be noticeable.
        /// </summary>
        [Tooltip("Duration to hold at full opacity before transitioning back")]
        [SerializeField] private float holdDuration = 0.2f;

        /// <summary>
        /// The Image component that will be faded. This should be assigned in the inspector
        /// and should be on the same GameObject as the CanvasGroup or one of its children.
        /// </summary>
        [Tooltip("Image component to apply fade color to")]
        [SerializeField] private UnityEngine.UI.Image fadeImage;

        // Internal state tracking
        private Coroutine loadProgressCoroutine;
        private FadeUtility fadeUtility;

        /// <summary>
        /// Shows the loading screen by fading in from transparent to opaque.
        /// This is called automatically by the LoadingScreenManager when transitioning between scenes.
        /// </summary>
        /// <returns>A Task that completes when the fade in is finished</returns>
        public override async Task ShowAsync()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            await fadeUtility.FadeAsync(canvasGroup, 0f, 1f, true);
        }

        /// <summary>
        /// Hides the loading screen by fading out from opaque to transparent.
        /// This is called automatically by the LoadingScreenManager after the new scene is loaded.
        /// </summary>
        /// <returns>A Task that completes when the fade out is finished</returns>
        public override async Task HideAsync()
        {
            await fadeUtility.FadeAsync(canvasGroup, 1f, 0f, false);
        }

        /// <summary>
        /// Called before scene loading begins. Sets up progress tracking and prepares for transition.
        /// </summary>
        /// <param name="currentScene">The scene being transitioned from</param>
        /// <param name="targetScene">The scene being loaded</param>
        /// <param name="loadMode">How the scene should be loaded (additive or single)</param>
        /// <returns>True to indicate this component will handle scene activation</returns>
        public override async Task<bool> OnBeforeSceneLoadAsync(Scene currentScene, string targetScene, LoadSceneMode loadMode)
        {
            await base.OnBeforeSceneLoadAsync(currentScene, targetScene, loadMode);

            // Start tracking load progress
            if (loadProgressCoroutine != null)
            {
                StopCoroutine(loadProgressCoroutine);
            }
            loadProgressCoroutine = StartCoroutine(TrackLoadProgress());

            return true; // We'll handle scene activation
        }

        /// <summary>
        /// Called after the new scene is loaded but before it's activated.
        /// Ensures smooth transition by adding a small delay.
        /// </summary>
        /// <param name="loadedScene">The scene that was just loaded</param>
        /// <returns>True when ready to activate the new scene</returns>
        public override async Task<bool> OnAfterSceneLoadAsync(Scene loadedScene)
        {
            if (loadProgressCoroutine != null)
            {
                StopCoroutine(loadProgressCoroutine);
                loadProgressCoroutine = null;
            }

            // Brief delay for visual polish
            await Task.Delay(500);

            return true; // Ready to activate the scene
        }

        /// <summary>
        /// Monitors scene loading progress in real-time.
        /// </summary>
        private IEnumerator TrackLoadProgress()
        {
            float startTime = Time.realtimeSinceStartup;
            Scene targetScene = default;
            float lastProgress = -1;

            // Wait for scene to become available
            while (!targetScene.IsValid())
            {
                targetScene = SceneManager.GetSceneByName(targetSceneName);
                yield return new WaitForSeconds(0.05f);
            }

            // Monitor loading progress
            while (!targetScene.isLoaded)
            {
                float loadProgress = SceneManager.GetSceneByName(targetSceneName).GetHashCode() != 0 ? 0.9f : 0f;
                
                if (Mathf.Abs(loadProgress - lastProgress) > 0.05f)
                {
                    lastProgress = loadProgress;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        /// <summary>
        /// Initializes the component and sets up the fade utility.
        /// Validates required components are properly assigned.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            if (fadeImage == null)
            {
                Debug.LogError("[FadeLoading] No fade image assigned. Please assign an Image component in the inspector.");
                return;
            }

            fadeUtility = new FadeUtility(fadeDuration, holdDuration, this);
        }

        /// <summary>
        /// Ensures proper cleanup of coroutines when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (loadProgressCoroutine != null)
            {
                StopCoroutine(loadProgressCoroutine);
            }
            fadeUtility?.StopFade();
        }
    }
}
