using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using MenuFlow.ScriptableObjects;
using MenuFlow.Components;
using System.IO;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Editor window for creating new menu panels.
    /// Handles the creation of both MenuDefinition assets and panel prefabs with proper setup.
    /// </summary>
    public class PanelCreatorWindow : EditorWindow
    {
        private TextField panelNameField;
        private string sceneName;
        private System.Action<MenuDefinition> onPanelCreated;

        /// <summary>
        /// Shows the panel creator window.
        /// </summary>
        /// <param name="sceneName">Name of the scene the panel belongs to.</param>
        /// <param name="onPanelCreated">Callback invoked when panel creation is complete.</param>
        public static void ShowWindow(string sceneName, System.Action<MenuDefinition> onPanelCreated)
        {
            var window = GetWindow<PanelCreatorWindow>(true, "Create Panel");
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(300, 150);
            window.sceneName = sceneName;
            window.onPanelCreated = onPanelCreated;
        }

        /// <summary>
        /// Creates and configures the window's UI elements.
        /// </summary>
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            // Panel name field
            panelNameField = new TextField("Panel Name");
            panelNameField.style.marginBottom = 20;
            root.Add(panelNameField);

            // Create button
            var createButton = new Button(CreatePanel) { text = "Create Panel" };
            createButton.style.height = 30;
            root.Add(createButton);
        }

        /// <summary>
        /// Creates a new menu panel with all required components and assets.
        /// This includes:
        /// - MenuDefinition ScriptableObject
        /// - Panel prefab with proper UI setup
        /// - Scene instance if a scene is specified
        /// </summary>
        private void CreatePanel()
        {
            if (string.IsNullOrEmpty(panelNameField.value))
            {
                EditorUtility.DisplayDialog("Error", "Panel name cannot be empty", "OK");
                return;
            }

            // Create the ScriptableObject first
            var menuDefinition = CreateInstance<MenuDefinition>();
            menuDefinition.name = panelNameField.value;

            // Ensure directories exist
            var prefabPath = $"Assets/Prefabs/UI/Panels";
            var scriptableObjectPath = $"Assets/ScriptableObjects/UI/Panels";

            Directory.CreateDirectory(prefabPath);
            Directory.CreateDirectory(scriptableObjectPath);

            // Save the ScriptableObject
            AssetDatabase.CreateAsset(menuDefinition,
                $"{scriptableObjectPath}/{panelNameField.value}.asset");

            // Create the panel prefab with proper UI setup
            var panelPrefab = new GameObject(panelNameField.value);

            // Add RectTransform
            var rectTransform = panelPrefab.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for fading
            var canvasGroup = panelPrefab.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Add MenuPanel component
            var menuPanel = panelPrefab.AddComponent<MenuPanel>();

            // Assign the MenuDefinition to the panel
            var serializedPrefab = new SerializedObject(menuPanel);
            var menuDefinitionProperty = serializedPrefab.FindProperty("menuDefinition");
            menuDefinitionProperty.objectReferenceValue = menuDefinition;
            serializedPrefab.ApplyModifiedProperties();

            // Save the prefab
            var prefabAsset = PrefabUtility.SaveAsPrefabAsset(panelPrefab,
                $"{prefabPath}/{panelNameField.value}.prefab");
            DestroyImmediate(panelPrefab);

            // Use SerializedObject to modify the MenuDefinition fields
            var serializedObject = new SerializedObject(menuDefinition);
            var prefabProperty = serializedObject.FindProperty("menuPrefab");
            var sceneNameProperty = serializedObject.FindProperty("sceneName");
            var sceneInstanceProperty = serializedObject.FindProperty("sceneInstance");

            prefabProperty.objectReferenceValue = prefabAsset;
            sceneNameProperty.stringValue = sceneName;

            // Create instance in scene if needed
            if (!string.IsNullOrEmpty(sceneName))
            {
                var sceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                sceneInstance.name = panelNameField.value;
                sceneInstanceProperty.objectReferenceValue = sceneInstance;

                // Make sure the scene instance has the MenuDefinition assigned
                var sceneMenuPanel = sceneInstance.GetComponent<MenuPanel>();
                var serializedScenePanel = new SerializedObject(sceneMenuPanel);
                var sceneMenuDefinitionProperty = serializedScenePanel.FindProperty("menuDefinition");
                sceneMenuDefinitionProperty.objectReferenceValue = menuDefinition;
                serializedScenePanel.ApplyModifiedProperties();

                // Save the scene to persist the instantiated prefab
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(menuDefinition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            onPanelCreated?.Invoke(menuDefinition);
            Close();
        }
    }
}