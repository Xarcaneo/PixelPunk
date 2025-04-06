using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using MenuFlow.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace MenuFlow.Editor
{ 
    /// <summary>
    /// Editor window for the MenuFlow system that provides a visual graph interface for managing menu connections.
    /// Handles scene selection, graph visualization, and menu system state management.
    /// </summary>
    public class MenuFlowEditorWindow : EditorWindow
    {
        private IMenuFlowGraphView graphView;
        private MenuFlowState state;
        private MenuFlowGraphManager graphManager;
        private ToolbarMenu sceneDropdown;

        /// <summary>
        /// Shows or focuses the MenuFlow Editor window
        /// </summary>
        [MenuItem("Window/MenuFlow/Menu Flow Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<MenuFlowEditorWindow>();
            window.titleContent = new GUIContent("Menu Flow Editor");
            window.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// Initializes the editor window when it is enabled.
        /// Sets up the UI components, state management, and graph visualization.
        /// </summary>
        private void OnEnable()
        {
            // Add style sheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.menuflow.ui/Editor/MenuFlow.uss");
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            rootVisualElement.AddToClassList("menu-flow-editor");

            state = new MenuFlowState();
            state.OnMenuSystemChanged += OnMenuSystemChanged;
            
            ConstructGraphView();
            graphManager = new MenuFlowGraphManager(graphView, state);
            GenerateToolbar();
            state.LoadMenuSystem();
            
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// Cleans up resources and event subscriptions when the window is disabled
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (state != null)
            {
                state.OnMenuSystemChanged -= OnMenuSystemChanged;
            }
            if (graphManager != null)
            {
                graphManager.Cleanup();
            }
        }

        /// <summary>
        /// Handles menu system state changes by updating the scene selection dropdown
        /// </summary>
        private void OnMenuSystemChanged()
        {
            UpdateSceneDropdown();
        }

        /// <summary>
        /// Updates the graph view when Unity's editor updates.
        /// Checks for modifications to panels in the current scene and refreshes the graph accordingly.
        /// </summary>
        private void OnEditorUpdate()
        {
            if (!state.IsValid) return;

            // Check if any panel in the current scene has been modified
            var currentPanels = state.GetCurrentScenePanels().ToList();
            var currentNodeKeys = graphView.NodeMap.Keys.ToList();

            // Check for new or removed panels
            var panelNames = currentPanels.Select(p => p.name).ToList();
            var addedPanels = panelNames.Except(currentNodeKeys).ToList();
            var removedPanels = currentNodeKeys.Except(panelNames).ToList();

            if (addedPanels.Any() || removedPanels.Any())
            {
                graphManager.RefreshGraph(currentPanels, addedPanels, removedPanels);
            }
        }

        /// <summary>
        /// Updates the scene selection dropdown with available scenes from the menu system.
        /// Marks the current scene as checked in the dropdown menu.
        /// </summary>
        private void UpdateSceneDropdown()
        {
            if (!state.IsValid || sceneDropdown == null) return;

            sceneDropdown.menu.MenuItems().Clear();
            
            // Add scene options
            foreach (var scene in state.MenuSystem.scenes)
            {
                var sceneName = scene.sceneName;
                sceneDropdown.menu.AppendAction(sceneName, (a) => {
                    state.SelectScene(sceneName);
                }, (a) => state.SelectedScene == sceneName ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }

            // Update dropdown text to show current scene
            sceneDropdown.text = state.SelectedScene ?? "Select Scene";
        }

        /// <summary>
        /// Creates and initializes the graph view component
        /// </summary>
        private void ConstructGraphView()
        {
            graphView = new MenuFlowGraphView(this);
            graphView.GraphView.StretchToParentSize();
            rootVisualElement.Add(graphView.GraphView);
        }

        /// <summary>
        /// Creates and initializes the toolbar with scene selection dropdown
        /// </summary>
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.AddToClassList("menu-flow-toolbar");

            // Scene selection dropdown
            sceneDropdown = new ToolbarMenu { text = "Select Scene" };
            toolbar.Add(sceneDropdown);

            // Add flexible space
            var spacer = new ToolbarSpacer();
            spacer.AddToClassList("menu-flow-toolbar__spacer");
            toolbar.Add(spacer);

            rootVisualElement.Add(toolbar);
        }
    }
}