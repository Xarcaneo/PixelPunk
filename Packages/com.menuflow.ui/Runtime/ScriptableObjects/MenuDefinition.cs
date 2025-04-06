using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MenuFlow.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines a menu in the UI hierarchy.
    /// Contains identifying information, structural links, and transition settings.
    /// </summary>
    [CreateAssetMenu(fileName = "Menu", menuName = "MenuFlow/Menu Definition")]
    public class MenuDefinition : ScriptableObject, ISerializationCallbackReceiver
    {
        [System.Serializable]
        public class NodeData
        {
            public Vector2 position;
            public List<string> outputConnections = new List<string>();
        }

        [System.Serializable]
        private class SerializableNodeData
        {
            public string nodeId;
            public NodeData data;

            public SerializableNodeData(string id, NodeData nodeData)
            {
                nodeId = id;
                data = nodeData;
            }
        }

        [Header("Menu Hierarchy")]
        /// <summary>
        /// Reference to this menu's parent in the menu hierarchy.
        /// </summary>
        [SerializeField] public MenuDefinition parentMenu;

        /// <summary>
        /// List of menus that are direct children of this menu.
        /// </summary>
        [SerializeField] public List<MenuDefinition> childMenus = new();
        
        [Header("Scene & UI References")]
        /// <summary>
        /// Optional scene name that contains this menu.
        /// If not set, assumes menu is in the current scene.
        /// </summary>
        [Tooltip("Optional - Scene that contains this menu. If not set, assumes menu is in current scene.")]
        [SerializeField] internal string sceneName;

        /// <summary>
        /// Prefab that contains the menu's UI elements and components.
        /// </summary>
        [SerializeField] internal GameObject menuPrefab;

        /// <summary>
        /// Optional reference to a pre-existing instance in the scene.
        /// Will use prefab if not set.
        /// </summary>
        [Tooltip("Optional - Reference to an instance in the scene. Will use prefab if not set.")]
        [SerializeField] internal GameObject sceneInstance;
        
        // Properties
        /// <summary>
        /// Gets the parent menu in the hierarchy.
        /// </summary>
        public MenuDefinition ParentMenu => parentMenu;

        /// <summary>
        /// Gets a read-only list of child menus.
        /// </summary>
        public IReadOnlyList<MenuDefinition> ChildMenus => childMenus;

        /// <summary>
        /// Gets the name of the scene containing this menu.
        /// </summary>
        public string SceneName => sceneName;

        /// <summary>
        /// Gets the prefab used to instantiate this menu.
        /// </summary>
        public GameObject MenuPrefab => menuPrefab;

        /// <summary>
        /// Gets the pre-existing scene instance if available.
        /// </summary>
        public GameObject SceneInstance => sceneInstance;

        // Runtime state (not serialized)
        private GameObject currentInstance;
        /// <summary>
        /// Gets or sets the current runtime instance of this menu.
        /// This is not serialized and only exists during runtime.
        /// </summary>
        public GameObject CurrentInstance
        {
            get => currentInstance;
            set => currentInstance = value;
        }

        [SerializeField] private List<SerializableNodeData> serializedNodeData = new List<SerializableNodeData>();
        private Dictionary<string, NodeData> nodeData = new Dictionary<string, NodeData>();

        public void OnBeforeSerialize()
        {
            // Convert dictionary to serializable list before saving
            serializedNodeData.Clear();
            foreach (var kvp in nodeData)
            {
                serializedNodeData.Add(new SerializableNodeData(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            // Convert serialized data to dictionary when asset is loaded
            nodeData.Clear();
            foreach (var data in serializedNodeData)
            {
                if (data != null && !string.IsNullOrEmpty(data.nodeId))
                {
                    nodeData[data.nodeId] = data.data;
                }
            }
        }

        public void SetNodePosition(string nodeId, Vector2 position)
        {
            if (!nodeData.ContainsKey(nodeId))
            {
                nodeData[nodeId] = new NodeData();
            }
            nodeData[nodeId].position = position;
            serializedNodeData.Clear();
            foreach (var kvp in nodeData)
            {
                serializedNodeData.Add(new SerializableNodeData(kvp.Key, kvp.Value));
            }
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public Vector2 GetNodePosition(string nodeId)
        {
            if (nodeData.ContainsKey(nodeId))
            {
                return nodeData[nodeId].position;
            }
            return Vector2.zero;
        }

        public void SetNodeConnections(string nodeId, List<string> outputConnections)
        {
            if (!nodeData.ContainsKey(nodeId))
            {
                nodeData[nodeId] = new NodeData();
            }
            nodeData[nodeId].outputConnections = outputConnections;
            serializedNodeData.Clear();
            foreach (var kvp in nodeData)
            {
                serializedNodeData.Add(new SerializableNodeData(kvp.Key, kvp.Value));
            }
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public List<string> GetNodeConnections(string nodeId)
        {
            return nodeData.ContainsKey(nodeId) ? nodeData[nodeId].outputConnections : new List<string>();
        }

        public void ClearNodeData()
        {
            nodeData.Clear();
            serializedNodeData.Clear();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Validates the menu definition in editor and runtime.
        /// Prevents circular references and ensures parent-child relationships are consistent.
        /// </summary>
        private void OnValidate()
        {
            // Prevent self-reference in parent
            if (parentMenu == this)
            {
                Debug.LogError($"Menu '{name}' cannot be its own parent!", this);
                parentMenu = null;
            }

            // Prevent circular references in children
            if (childMenus.Contains(this))
            {
                Debug.LogError($"Menu '{name}' cannot be its own child!", this);
                childMenus.Remove(this);
            }

            // Ensure child menus have this as their parent
            foreach (var child in childMenus)
            {
                if (child != null && child.parentMenu != this)
                {
                    child.parentMenu = this;
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(child);
                    #endif
                }
            }
        }

        /// <summary>
        /// Adds a child menu to this menu's hierarchy.
        /// Updates the parent-child relationships and marks objects as dirty in the editor.
        /// </summary>
        /// <param name="childMenu">The menu to add as a child.</param>
        public void AddChildMenu(MenuDefinition childMenu)
        {
            if (childMenu == null || childMenu == this || childMenus.Contains(childMenu))
                return;

            childMenus.Add(childMenu);
            childMenu.parentMenu = this;
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(childMenu);
            #endif
        }

        /// <summary>
        /// Removes a child menu from this menu's hierarchy.
        /// Updates the parent-child relationships and marks objects as dirty in the editor.
        /// </summary>
        /// <param name="childMenu">The menu to remove from children.</param>
        public void RemoveChildMenu(MenuDefinition childMenu)
        {
            if (childMenu == null || !childMenus.Contains(childMenu))
                return;

            childMenus.Remove(childMenu);
            if (childMenu.parentMenu == this)
            {
                childMenu.parentMenu = null;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(childMenu);
                #endif
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Checks if this menu is an ancestor of the specified menu.
        /// Traverses up the menu hierarchy to find a match.
        /// </summary>
        /// <param name="menu">The menu to check ancestry for.</param>
        /// <returns>True if this menu is an ancestor of the specified menu, false otherwise.</returns>
        public bool IsAncestorOf(MenuDefinition menu)
        {
            if (menu == null)
                return false;

            var current = menu.parentMenu;
            while (current != null)
            {
                if (current == this)
                    return true;
                current = current.parentMenu;
            }
            return false;
        }
    }
}
