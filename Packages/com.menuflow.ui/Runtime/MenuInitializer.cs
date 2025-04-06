using UnityEngine;

namespace MenuFlow
{
    /// <summary>
    /// Initializes the MenuFlow system at runtime.
    /// Handles the creation and setup of the MenuManager singleton.
    /// </summary>
    public class MenuInitializer : MonoBehaviour
    {
        /// <summary>
        /// Automatically called before scene load to initialize the MenuManager.
        /// Creates the MenuManager instance from a prefab in the Resources folder.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var menuManagerPrefab = Resources.Load<GameObject>("MenuManager");
            
            if (menuManagerPrefab == null)
            {
                Debug.LogError("MenuManager prefab not found. Make sure MenuManager.prefab exists in a Resources folder.");
                return;
            }

            var instance = Instantiate(menuManagerPrefab);
            instance.name = "MenuManager";
            DontDestroyOnLoad(instance);
        }
    }
}
