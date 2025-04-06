using UnityEngine;
using UnityEditor;
using MenuFlow.ScriptableObjects;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Manages the state and operations of the MenuFlow editor.
    /// Handles loading, saving, and tracking of MenuSystem and scene selection.
    /// </summary>
    public class MenuFlowState
    {
        private MenuSystem menuSystem;
        private string selectedScene;

        /// <summary>
        /// Event triggered when the MenuSystem state changes
        /// </summary>
        public event Action OnMenuSystemChanged;

        /// <summary>
        /// Event triggered when the selected scene changes
        /// </summary>
        public event Action OnSceneChanged;

        /// <summary>
        /// Gets the current MenuSystem instance
        /// </summary>
        public MenuSystem MenuSystem => menuSystem;

        /// <summary>
        /// Gets whether the current MenuSystem is valid and loaded
        /// </summary>
        public bool IsValid => menuSystem != null;

        /// <summary>
        /// Gets the currently selected scene name
        /// </summary>
        public string SelectedScene => selectedScene;

        /// <summary>
        /// Gets the current scene entry if one is selected
        /// </summary>
        public MenuSystem.SceneEntry CurrentScene => 
            IsValid && !string.IsNullOrEmpty(selectedScene) 
            ? menuSystem.scenes.FirstOrDefault(s => s.sceneName == selectedScene) 
            : null;

        /// <summary>
        /// Loads the MenuSystem asset from the project
        /// </summary>
        public void LoadMenuSystem()
        {
            string[] guids = AssetDatabase.FindAssets("t:MenuSystem");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var newSystem = AssetDatabase.LoadAssetAtPath<MenuSystem>(path);
                if (newSystem != menuSystem)
                {
                    menuSystem = newSystem;
                    if (menuSystem != null && menuSystem.scenes.Count > 0)
                    {
                        SelectScene(menuSystem.scenes[0].sceneName);
                    }
                    OnMenuSystemChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Selects a scene by name
        /// </summary>
        public void SelectScene(string sceneName)
        {
            if (selectedScene != sceneName)
            {
                selectedScene = sceneName;
                OnSceneChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets all panels in the current scene
        /// </summary>
        public MenuDefinition[] GetCurrentScenePanels()
        {
            var scene = CurrentScene;
            return scene?.panels?.Where(p => p != null).ToArray() ?? Array.Empty<MenuDefinition>();
        }
    }
}
