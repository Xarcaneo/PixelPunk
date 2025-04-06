using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using MenuFlow.ScriptableObjects;
using MenuFlow.Components;
using MenuFlow.Editor.CodeGeneration;
using MenuFlow.Editor.Utilities;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.UI;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Editor window for managing MenuSystem configurations.
    /// Provides a user interface for editing menu panels, scenes, and their relationships.
    /// Follows the Single Responsibility Principle by delegating state management and panel tracking to specialized classes.
    /// </summary>
    public class MenuSystemWindowUI : EditorWindow
    {
        private MenuSystemState menuSystemState;
        private PanelTracker panelTracker;
        private VisualElement root;
        private ScrollView sceneList;
        private ObjectField menuSystemField;
        private bool isInitialized;
        private VisualElement tabContainer;
        private VisualElement initialSetupTab;
        private VisualElement scenesTab;
        private UnityEngine.UIElements.Button initialSetupButton;
        private UnityEngine.UIElements.Button scenesButton;
        private UnityEngine.UIElements.Button generateConstantsButton;
        private VisualElement constantsStatusIcon;

        private const string CONSTANTS_UP_TO_DATE_CLASS = "constants-up-to-date";
        private const string CONSTANTS_OUT_OF_DATE_CLASS = "constants-out-of-date";

        /// <summary>
        /// Shows or focuses the MenuSystem window.
        /// </summary>
        [MenuItem("Window/MenuFlow/Menu System")]
        public static void ShowWindow()
        {
            var window = GetWindow<MenuSystemWindowUI>();
            window.titleContent = new GUIContent("Menu System");
            window.minSize = new Vector2(450, 200);
        }

        /// <summary>
        /// Creates and initializes the window's UI elements.
        /// This is called by Unity when the window is first opened.
        /// </summary>
        private void CreateGUI()
        {
            root = rootVisualElement;
            
            // Add USS for the constants status icon
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.menuflow.ui/Editor/MenuFlow.uss");
            root.styleSheets.Add(styleSheet);

            // Initialize state and subscribe to changes
            menuSystemState = new MenuSystemState();
            menuSystemState.OnMenuSystemChanged += UpdateConstantsStatus;
            panelTracker = new PanelTracker(menuSystemState.Current);

            // Add the constants generation button and status icon to the toolbar
            var toolbar = new Toolbar();
            root.Add(toolbar);

            var constantsContainer = new VisualElement();
            constantsContainer.style.flexDirection = FlexDirection.Row;
            constantsContainer.style.alignItems = Align.Center;
            toolbar.Add(constantsContainer);

            generateConstantsButton = new UnityEngine.UIElements.Button(GenerateConstants) { text = "Generate Constants" };
            constantsContainer.Add(generateConstantsButton);

            constantsStatusIcon = new VisualElement();
            constantsStatusIcon.AddToClassList("constants-status-icon");
            constantsContainer.Add(constantsStatusIcon);

            // Add style
            var styleSheet2 = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.menuflow.ui/Editor/MenuSystemWindow.uss");
            if (styleSheet2 != null)
                root.styleSheets.Add(styleSheet2);
            
            // Create main layout
            var mainLayout = new VisualElement();
            mainLayout.AddToClassList("main-layout");
            root.Add(mainLayout);

            // Add MenuSystem field
            menuSystemField = new ObjectField("Menu System")
            {
                objectType = typeof(MenuSystem),
                allowSceneObjects = false
            };
            menuSystemField.AddToClassList("menu-system-field");
            menuSystemField.RegisterValueChangedCallback(OnMenuSystemChanged);
            mainLayout.Add(menuSystemField);

            // Create tab buttons container
            var tabButtons = new VisualElement();
            tabButtons.AddToClassList("tab-buttons");
            mainLayout.Add(tabButtons);

            // Create Scenes tab button (first)
            scenesButton = new UnityEngine.UIElements.Button(() => SwitchTab(scenesTab)) { text = "Scenes" };
            scenesButton.AddToClassList("tab-button");
            tabButtons.Add(scenesButton);

            // Create Initial Setup tab button (second)
            initialSetupButton = new UnityEngine.UIElements.Button(() => SwitchTab(initialSetupTab)) { text = "Initial Setup" };
            initialSetupButton.AddToClassList("tab-button");
            tabButtons.Add(initialSetupButton);

            // Create tab container
            tabContainer = new VisualElement();
            tabContainer.AddToClassList("tab-container");
            mainLayout.Add(tabContainer);

            // Create Scenes tab (first)
            scenesTab = new VisualElement();
            scenesTab.AddToClassList("tab-content");
            sceneList = new ScrollView();
            sceneList.AddToClassList("scene-list");
            scenesTab.Add(sceneList);
            tabContainer.Add(scenesTab);

            // Create Initial Setup tab (second)
            initialSetupTab = new VisualElement();
            initialSetupTab.AddToClassList("tab-content");
            initialSetupTab.style.display = DisplayStyle.None;
            tabContainer.Add(initialSetupTab);

            // Set initial active tab to Scenes
            SwitchTab(scenesTab);

            InitializeState();
            isInitialized = true;
        }

        private void SwitchTab(VisualElement targetTab)
        {
            // Hide all tabs
            initialSetupTab.style.display = DisplayStyle.None;
            scenesTab.style.display = DisplayStyle.None;

            // Show target tab
            targetTab.style.display = DisplayStyle.Flex;

            // Update button states
            initialSetupButton.RemoveFromClassList("tab-button--active");
            scenesButton.RemoveFromClassList("tab-button--active");

            if (targetTab == initialSetupTab)
                initialSetupButton.AddToClassList("tab-button--active");
            else
                scenesButton.AddToClassList("tab-button--active");
        }

        /// <summary>
        /// Initializes the MenuSystemState and PanelTracker components.
        /// This is called after UI elements are created to ensure proper initialization order.
        /// </summary>
        private void InitializeState()
        {
            if (menuSystemField == null) return; // Guard against UI not being ready

            // Initialize MenuSystemState
            menuSystemState.OnMenuSystemChanged += OnMenuSystemStateChanged;
            menuSystemField.value = menuSystemState.Current;

            // Initialize PanelTracker
            if (panelTracker != null)
            {
                panelTracker.Dispose();
            }
            panelTracker = new PanelTracker(menuSystemState.Current);
            panelTracker.OnPanelChanged += OnPanelChanged;

            RefreshSceneList();
        }

        /// <summary>
        /// Called when the window becomes enabled and active.
        /// Ensures proper state initialization when the window is focused.
        /// </summary>
        private void OnEnable()
        {
            // Only initialize if GUI is ready
            if (isInitialized && menuSystemState == null)
            {
                InitializeState();
            }
            if (menuSystemState != null)
            {
                menuSystemState.OnMenuSystemChanged += UpdateConstantsStatus;
            }
        }

        /// <summary>
        /// Called when the window becomes disabled or inactive.
        /// Cleans up resources and unsubscribes from events.
        /// </summary>
        private void OnDisable()
        {
            if (panelTracker != null)
            {
                panelTracker.Dispose();
                panelTracker = null;
            }

            if (menuSystemState != null)
            {
                menuSystemState.OnMenuSystemChanged -= OnMenuSystemStateChanged;
                menuSystemState.OnMenuSystemChanged -= UpdateConstantsStatus;
                menuSystemState = null;
            }
        }

        private void OnDestroy()
        {
            if (menuSystemState != null)
            {
                menuSystemState.OnMenuSystemChanged -= UpdateConstantsStatus;
            }
        }

        /// <summary>
        /// Handles MenuSystem state changes by updating the UI and panel tracker.
        /// </summary>
        private void OnMenuSystemStateChanged()
        {
            if (!isInitialized) return;

            // Update UI elements
            menuSystemField.value = menuSystemState.Current;
            
            // Update panel tracker
            panelTracker = new PanelTracker(menuSystemState.Current);
            
            // Refresh scene list
            RefreshSceneList();
            
            // Update constants status
            UpdateConstantsStatus();
        }

        /// <summary>
        /// Handles panel visibility changes by updating the UI.
        /// </summary>
        private void OnPanelChanged()
        {
            Repaint();
            RefreshSceneList();
        }

        /// <summary>
        /// Handles changes to the MenuSystem field in the UI.
        /// </summary>
        private void OnMenuSystemChanged(ChangeEvent<Object> evt)
        {
            menuSystemState.SetMenuSystem(evt.newValue as MenuSystem);
            UpdateConstantsStatus();
        }

        /// <summary>
        /// Refreshes the scene list UI with current MenuSystem data.
        /// Updates panel visibility indicators and maintains scene hierarchy.
        /// </summary>
        private void RefreshSceneList()
        {
            if (sceneList == null || !menuSystemState.IsValid)
            {
                return;
            }
                
            sceneList.Clear();
            initialSetupTab.Clear();

            // Add initial panel section to Initial Setup tab
            var initialPanelSection = new Box();
            initialPanelSection.AddToClassList("section");
            
            var initialPanelHeader = new Label("Initial Panel");
            initialPanelHeader.AddToClassList("section-header");
            initialPanelSection.Add(initialPanelHeader);

            var initialPanelField = new ObjectField()
            {
                objectType = typeof(MenuDefinition),
                allowSceneObjects = false,
                value = menuSystemState.Current.initialPanel
            };
            if (panelTracker?.CurrentPanel == menuSystemState.Current.initialPanel)
            {
                initialPanelField.AddToClassList("active-panel");
            }
            initialPanelField.RegisterValueChangedCallback(evt => 
            {
                menuSystemState.UpdateInitialPanel(evt.newValue as MenuDefinition);
            });
            initialPanelSection.Add(initialPanelField);
            initialSetupTab.Add(initialPanelSection);

            // Add loading screen prefab field to Initial Setup tab
            var loadingScreenSection = new Box();
            loadingScreenSection.AddToClassList("section");
            loadingScreenSection.AddToClassList("loading-screen-section");
            
            var loadingScreenHeader = new Label("Loading Screen Prefab");
            loadingScreenHeader.AddToClassList("section-header");
            loadingScreenSection.Add(loadingScreenHeader);

            var loadingScreenField = new ObjectField()
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = menuSystemState.Current.loadingScreenPrefab
            };
            loadingScreenField.RegisterValueChangedCallback(evt => 
            {
                menuSystemState.UpdateLoadingScreenPrefab(evt.newValue as GameObject);
            });
            loadingScreenSection.Add(loadingScreenField);
            initialSetupTab.Add(loadingScreenSection);

            // Add scene entries to Scenes tab
            foreach (var scene in menuSystemState.Current.scenes)
            {
                if (scene == null)
                    continue;
                
                var sceneBox = new Box();
                sceneBox.AddToClassList("scene-box");

                // Scene header
                var header = new VisualElement();
                header.AddToClassList("scene-header");
                
                var sceneField = new ObjectField()
                {
                    objectType = typeof(SceneAsset),
                    allowSceneObjects = false,
                    value = scene.sceneAsset
                };
                sceneField.AddToClassList("scene-name");
                
                // Lock the field if scene is already assigned
                if (scene.sceneAsset != null)
                {
                    sceneField.SetEnabled(false);
                }
                else
                {
                    // Setup drag and drop only for unassigned fields
                    var fieldInput = sceneField.Q(className: "unity-object-field__input");
                    fieldInput.RegisterCallback<DragUpdatedEvent>(e => 
                    {
                        if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is SceneAsset)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            e.StopPropagation();
                        }
                    });
                    
                    fieldInput.RegisterCallback<DragPerformEvent>(e => 
                    {
                        if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is SceneAsset sceneAsset)
                        {
                            scene.sceneAsset = sceneAsset;
                            scene.sceneName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneAsset));
                            EditorUtility.SetDirty(menuSystemState.Current);
                            sceneField.value = sceneAsset;
                            sceneField.SetEnabled(false); // Lock the field after assigning
                            e.StopPropagation();
                        }
                    });

                    sceneField.RegisterValueChangedCallback(evt =>
                    {
                        var sceneAsset = evt.newValue as SceneAsset;
                        if (sceneAsset != null)
                        {
                            scene.sceneAsset = sceneAsset;
                            scene.sceneName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneAsset));
                            EditorUtility.SetDirty(menuSystemState.Current);
                            sceneField.SetEnabled(false); // Lock the field after assigning
                        }
                    });
                }
                header.Add(sceneField);

                var buttonContainer = new VisualElement();
                buttonContainer.style.flexDirection = FlexDirection.Row;

                // Play button to open scene
                var playButton = new UnityEngine.UIElements.Button(() => 
                {
                    if (scene.sceneAsset != null)
                    {
                        var path = AssetDatabase.GetAssetPath(scene.sceneAsset);
                        // Check for unsaved changes
                        if (EditorSceneManager.GetActiveScene().isDirty)
                        {
                            if (EditorUtility.DisplayDialog("Save Scene?", 
                                "The current scene has unsaved changes. Do you want to save them before switching scenes?", 
                                "Save", "Don't Save"))
                            {
                                EditorSceneManager.SaveOpenScenes();
                            }
                        }

                        // Open the scene
                        if (System.IO.File.Exists(path))
                        {
                            EditorSceneManager.OpenScene(path);
                        }
                    }
                })
                {
                    text = "▶"
                };
                playButton.AddToClassList("small-button");
                playButton.AddToClassList("play-button");
                buttonContainer.Add(playButton);

                // Delete button
                var deleteButton = new UnityEngine.UIElements.Button(() =>
                {
                    menuSystemState.Current.scenes.Remove(scene);
                    EditorUtility.SetDirty(menuSystemState.Current);
                    RefreshSceneList();
                })
                {
                    text = "Delete"
                };
                deleteButton.AddToClassList("delete-button");
                buttonContainer.Add(deleteButton);

                header.Add(buttonContainer);
                
                sceneBox.Add(header);

                // Panels list
                var panelsList = new ListView()
                {
                    reorderable = true,
                    showBorder = true,
                    showFoldoutHeader = true,
                    headerTitle = "Panels",
                    showAddRemoveFooter = true,
                    fixedItemHeight = 20
                };
                panelsList.AddToClassList("panels-list");
                
                panelsList.makeItem = () => new ObjectField() { objectType = typeof(MenuDefinition), allowSceneObjects = false };
                panelsList.bindItem = (element, index) => 
                {
                    var field = element as ObjectField;
                    var panel = scene.panels[index];
                    field.value = panel;
                    
                    // Clear existing class to avoid stale highlighting
                    field.RemoveFromClassList("active-panel");
                    
                    // Add highlight if this is the current panel
                    if (panel == panelTracker.CurrentPanel)
                    {
                        field.AddToClassList("active-panel");
                    }
                };
                panelsList.itemsSource = scene.panels;

                // Override default add/remove behavior
                var addButton = panelsList.Q<UnityEngine.UIElements.Button>("unity-list-view__add-button");
                var currentScene = scene; // Capture scene in local variable
                addButton.clickable = new Clickable(() =>
                {
                    // First, check if we need to save the current scene
                    if (EditorSceneManager.GetActiveScene().isDirty)
                    {
                        if (EditorUtility.DisplayDialog("Save Scene?", 
                            "The current scene has unsaved changes. Do you want to save them before switching scenes?", 
                            "Save", "Don't Save"))
                        {
                            EditorSceneManager.SaveOpenScenes();
                        }
                    }

                    // Try to find or create the scene
                    if (currentScene.sceneAsset == null)
                    {
                        Debug.LogError("Cannot create panel: No scene asset assigned. Please assign a scene first.");
                        return;
                    }

                    var path = AssetDatabase.GetAssetPath(currentScene.sceneAsset);
                    if (!System.IO.File.Exists(path))
                    {
                        // Scene doesn't exist, create it
                        var newScene = EditorSceneManager.NewScene(
                            NewSceneSetup.DefaultGameObjects,
                            NewSceneMode.Single);
                        
                        // Make sure we have a Canvas
                        var canvasGO = new GameObject("Canvas");
                        var canvas = canvasGO.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvasGO.AddComponent<CanvasScaler>();
                        canvasGO.AddComponent<GraphicRaycaster>();

                        // Save the new scene
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        EditorSceneManager.SaveScene(newScene, path);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        // Scene exists, open it
                        EditorSceneManager.OpenScene(path);
                    }

                    // Show panel creator
                    PanelCreatorWindow.ShowWindow(currentScene.sceneName, (newPanel) =>
                    {
                        if (newPanel != null && menuSystemState.Current != null)
                        {
                            var targetScene = menuSystemState.Current.scenes.FirstOrDefault(s => s.sceneName == currentScene.sceneName);
                            if (targetScene != null)
                            {
                                targetScene.panels.Add(newPanel);
                                EditorUtility.SetDirty(menuSystemState.Current);
                                UpdateConstantsStatus();
                                RefreshSceneList();
                            }
                        }
                    });
                });

                panelsList.itemsRemoved += (items) =>
                {
                    foreach (var index in items)
                    {
                        var menuDef = scene.panels[index];
                        if (menuDef != null)
                        {
                            // Delete the prefab if it exists
                            var prefabPath = $"Assets/Prefabs/UI/Panels/{menuDef.name}.prefab";
                            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                            {
                                AssetDatabase.DeleteAsset(prefabPath);
                            }

                            // Delete the ScriptableObject
                            var soPath = AssetDatabase.GetAssetPath(menuDef);
                            if (!string.IsNullOrEmpty(soPath))
                            {
                                AssetDatabase.DeleteAsset(soPath);
                            }

                            // If we're in play mode and the panel is in the scene, destroy it
                            if (Application.isPlaying && menuDef.CurrentInstance != null)
                            {
                                Object.Destroy(menuDef.CurrentInstance);
                            }
                        }
                    }

                    EditorUtility.SetDirty(menuSystemState.Current);
                    UpdateConstantsStatus();
                    AssetDatabase.SaveAssets();
                };

                panelsList.reorderMode = ListViewReorderMode.Animated;
                panelsList.itemIndexChanged += (fromIndex, toIndex) =>
                {
                    var item = scene.panels[fromIndex];
                    scene.panels.RemoveAt(fromIndex);
                    scene.panels.Insert(toIndex, item);
                    EditorUtility.SetDirty(menuSystemState.Current);
                    UpdateConstantsStatus();
                };

                sceneBox.Add(panelsList);
                sceneList.Add(sceneBox);
            }

            // Add scene button
            var addSceneButton = new UnityEngine.UIElements.Button(() =>
            {
                menuSystemState.Current.scenes.Add(new MenuSystem.SceneEntry());
                EditorUtility.SetDirty(menuSystemState.Current);
                RefreshSceneList();
            })
            {
                text = "Add Scene"
            };
            addSceneButton.AddToClassList("add-scene-button");
            sceneList.Add(addSceneButton);
        }

        private void UpdateConstantsStatus()
        {
            if (menuSystemState?.Current == null)
            {
                constantsStatusIcon.RemoveFromClassList(CONSTANTS_UP_TO_DATE_CLASS);
                constantsStatusIcon.RemoveFromClassList(CONSTANTS_OUT_OF_DATE_CLASS);
                generateConstantsButton.SetEnabled(false);
                return;
            }

            generateConstantsButton.SetEnabled(true);
            bool isUpToDate = MenuConstantsChecker.AreConstantsUpToDate(menuSystemState.Current);
            
            constantsStatusIcon.RemoveFromClassList(isUpToDate ? CONSTANTS_OUT_OF_DATE_CLASS : CONSTANTS_UP_TO_DATE_CLASS);
            constantsStatusIcon.AddToClassList(isUpToDate ? CONSTANTS_UP_TO_DATE_CLASS : CONSTANTS_OUT_OF_DATE_CLASS);
            
            constantsStatusIcon.tooltip = isUpToDate ? 
                "Constants are up to date" : 
                "Constants need to be regenerated";
        }

        private void GenerateConstants()
        {
            if (menuSystemState?.Current == null) return;
            
            MenuConstantsGenerator.RegenerateConstants();
            UpdateConstantsStatus();
        }
    }
}
