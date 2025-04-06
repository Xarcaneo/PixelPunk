using UnityEngine;
using System.Collections.Generic;
using MenuFlow.Interfaces;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MenuFlow.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines the menu system structure for a game.
    /// Contains scene-specific menu panel configurations and the initial menu panel.
    /// </summary>
    [CreateAssetMenu(fileName = "MenuSystem", menuName = "MenuFlow/Menu System")]
    public class MenuSystem : ScriptableObject
    {
        /// <summary>
        /// Represents a collection of menu panels associated with a specific scene.
        /// </summary>
        [System.Serializable]
        public class SceneEntry
        {
#if UNITY_EDITOR
            /// <summary>
            /// The actual scene file.
            /// </summary>
            public SceneAsset sceneAsset;
#endif
            /// <summary>
            /// The name of the scene this entry is associated with.
            /// </summary>
            [SerializeField] public string sceneName;

            /// <summary>
            /// List of menu panels available in this scene.
            /// </summary>
            [SerializeField] public List<MenuDefinition> panels = new List<MenuDefinition>();
        }

        /// <summary>
        /// The menu panel that should be displayed when the game starts.
        /// This is the entry point for the menu system.
        /// </summary>
        [Header("Initial Menu")]
        [Tooltip("First menu to show when game starts")]
        [SerializeField] public MenuDefinition initialPanel;

#if UNITY_EDITOR
        /// <summary>
        /// The prefab to use as a loading screen during scene transitions.
        /// The prefab must have a component that implements ILoadingScreenComponent.
        /// </summary>
        [Header("Loading Screen")]
        [Tooltip("Prefab to show during loading (must have ILoadingScreenComponent)")]
        [SerializeField] public GameObject loadingScreenPrefab;
#endif
        /// <summary>
        /// Reference to the instantiated loading screen object.
        /// </summary>
        [HideInInspector]
        [SerializeField] private GameObject loadingScreenInstance;

        /// <summary>
        /// Collection of scene-specific menu configurations.
        /// Each entry maps a scene to its available menu panels.
        /// </summary>
        [Header("Scene Menus")]
        [SerializeField] public List<SceneEntry> scenes = new List<SceneEntry>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (loadingScreenPrefab != null)
            {
                var component = loadingScreenPrefab.GetComponent<ILoadingScreenComponent>();
                if (component == null)
                {
                    Debug.LogError($"Loading screen prefab '{loadingScreenPrefab.name}' must have a component that implements ILoadingScreenComponent!");
                    loadingScreenPrefab = null;
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
