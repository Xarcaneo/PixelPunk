using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using MenuFlow.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Manages graph operations for the MenuFlow editor.
    /// Handles node creation, positioning, and edge management.
    /// </summary>
    public class MenuFlowGraphManager
    {
        private IMenuFlowGraphView graphView;
        private MenuFlowState state;

        private const float HORIZONTAL_SPACING = 200f;
        private const float VERTICAL_SPACING = 150f;
        private const float ROOT_OFFSET_X = 100f;
        private const float ROOT_OFFSET_Y = 100f;
        private const float NODE_WIDTH = 150f;
        private const float NODE_HEIGHT = 120f;

        /// <summary>
        /// Initializes a new instance of the MenuFlowGraphManager.
        /// </summary>
        /// <param name="graphView">The graph view interface to manage</param>
        /// <param name="state">The menu flow state to track</param>
        public MenuFlowGraphManager(IMenuFlowGraphView graphView, MenuFlowState state)
        {
            this.graphView = graphView;
            this.state = state;
            state.OnSceneChanged += OnSceneChanged;
        }

        /// <summary>
        /// Performs cleanup operations, including saving node positions and unsubscribing from events.
        /// </summary>
        public void Cleanup()
        {
            if (state != null)
            {
                state.OnSceneChanged -= OnSceneChanged;
            }
            SaveAllNodePositions();
        }

        /// <summary>
        /// Handles scene change events by saving current positions and recreating nodes.
        /// </summary>
        private void OnSceneChanged()
        {
            SaveAllNodePositions(); // Save positions before changing scene
            CreateNodesFromCurrentScene();
        }

        /// <summary>
        /// Saves the positions of all nodes in the graph.
        /// Only marks nodes as dirty if their position has actually changed.
        /// </summary>
        public void SaveAllNodePositions()
        {
            if (!state.IsValid || graphView?.NodeMap == null) return;

            var validNodes = graphView.NodeMap.Values
                .Where(node => node != null && node is MenuNode menuNode && menuNode.Menu != null)
                .Cast<MenuNode>();

            bool anyNodeMoved = false;
            foreach (var menuNode in validNodes)
            {
                var rect = menuNode.GetPosition();
                if (menuNode.Menu.GetNodePosition(menuNode.Menu.name) != rect.position)
                {
                    menuNode.Menu.SetNodePosition(menuNode.Menu.name, rect.position);
                    EditorUtility.SetDirty(menuNode.Menu);
                    anyNodeMoved = true;
                }
            }

            if (anyNodeMoved)
            {
                EditorUtility.SetDirty(state.MenuSystem);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Creates nodes in the graph based on the current scene's panels.
        /// Performs creation in three passes:
        /// 1. Calculate positions for all valid panels
        /// 2. Create the actual nodes
        /// 3. Create edges between connected nodes
        /// </summary>
        public void CreateNodesFromCurrentScene()
        {
            if (!state.IsValid) return;

            graphView.ClearGraph();
            var nodePositions = new Dictionary<MenuDefinition, Vector2>();
            var currentPos = new Vector2(ROOT_OFFSET_X, ROOT_OFFSET_Y);

            // First pass: Create all nodes for valid panels
            var validPanels = state.GetCurrentScenePanels();
            foreach (var panel in validPanels)
            {
                var savedPos = panel.GetNodePosition(panel.name);
                nodePositions[panel] = savedPos != Vector2.zero ? savedPos : currentPos;
                currentPos.y += VERTICAL_SPACING;
            }

            // Second pass: Create nodes
            foreach (var panel in validPanels)
            {
                var nodePosition = nodePositions[panel];
                var node = new MenuNode(panel, nodePosition, graphView);
                graphView.AddNode(node);
                node.SetPosition(new Rect(nodePosition, new Vector2(NODE_WIDTH, NODE_HEIGHT)));
            }

            // Third pass: Create edges for valid connections
            foreach (var panel in validPanels)
            {
                if (!graphView.NodeMap.ContainsKey(panel.name)) continue;
                var sourceNode = graphView.NodeMap[panel.name];
                
                foreach (var childMenu in panel.childMenus.Where(c => c != null))
                {
                    if (graphView.NodeMap.TryGetValue(childMenu.name, out var targetNode))
                    {
                        CreateEdge(sourceNode, targetNode);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an edge between two menu nodes in the graph.
        /// </summary>
        /// <param name="sourceNode">The source node of the edge</param>
        /// <param name="targetNode">The target node of the edge</param>
        private void CreateEdge(MenuNode sourceNode, MenuNode targetNode)
        {
            var edge = new Edge
            {
                output = sourceNode.OutputPort,
                input = targetNode.InputPort
            };
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            graphView.GraphView.AddElement(edge);
        }

        /// <summary>
        /// Refreshes the graph when panels are added or removed.
        /// </summary>
        /// <param name="currentPanels">Current panels in the scene</param>
        /// <param name="addedPanels">Names of newly added panels</param>
        /// <param name="removedPanels">Names of removed panels</param>
        public void RefreshGraph(IEnumerable<MenuDefinition> currentPanels, List<string> addedPanels, List<string> removedPanels)
        {
            if (!state.IsValid) return;

            // Remove deleted nodes
            foreach (var panelName in removedPanels)
            {
                if (graphView.NodeMap.TryGetValue(panelName, out var node))
                {
                    graphView.GraphView.RemoveElement(node);
                    graphView.NodeMap.Remove(panelName);
                }
            }

            // Add new nodes
            foreach (var panelName in addedPanels)
            {
                var panel = currentPanels.FirstOrDefault(p => p.name == panelName);
                if (panel == null) continue;

                // Find a good position for the new node
                var position = CalculateNewNodePosition(panel);
                var node = new MenuNode(panel, position, graphView);
                graphView.AddNode(node);
                node.SetPosition(new Rect(position, new Vector2(NODE_WIDTH, NODE_HEIGHT)));

                // Create edges for the new node
                UpdateNodeConnections(node, panel);
            }

            SaveAllNodePositions();
        }

        /// <summary>
        /// Calculates a suitable position for a new node in the graph.
        /// Attempts to position near parent nodes if they exist.
        /// </summary>
        /// <param name="panel">The menu panel to position</param>
        /// <returns>The calculated position for the new node</returns>
        private Vector2 CalculateNewNodePosition(MenuDefinition panel)
        {
            // Try to position near parent nodes if they exist
            var parentNodes = graphView.NodeMap.Values
                .Where(n => n is MenuNode menuNode && 
                           menuNode.Menu != null && 
                           menuNode.Menu.childMenus.Contains(panel))
                .Cast<MenuNode>();

            if (parentNodes.Any())
            {
                var avgParentPos = parentNodes
                    .Select(n => n.GetPosition().position)
                    .Aggregate((a, b) => a + b) / parentNodes.Count();
                return avgParentPos + new Vector2(HORIZONTAL_SPACING, 0);
            }

            // Otherwise find a free space
            var existingPositions = graphView.NodeMap.Values
                .Select(n => n.GetPosition().position)
                .ToList();

            var position = new Vector2(ROOT_OFFSET_X, ROOT_OFFSET_Y);
            while (existingPositions.Any(p => Vector2.Distance(p, position) < VERTICAL_SPACING))
            {
                position += new Vector2(0, VERTICAL_SPACING);
            }

            return position;
        }

        /// <summary>
        /// Updates connections for a node, creating edges to both parent and child nodes.
        /// </summary>
        /// <param name="newNode">The node to update connections for</param>
        /// <param name="panel">The menu panel associated with the node</param>
        private void UpdateNodeConnections(MenuNode newNode, MenuDefinition panel)
        {
            // Connect to parent nodes
            foreach (var existingNode in graphView.NodeMap.Values)
            {
                var menuNode = existingNode as MenuNode;
                if (menuNode?.Menu == null) continue;

                if (menuNode.Menu.childMenus.Contains(panel))
                {
                    CreateEdge(menuNode, newNode);
                }
            }

            // Connect to child nodes
            foreach (var childMenu in panel.childMenus.Where(c => c != null))
            {
                if (graphView.NodeMap.TryGetValue(childMenu.name, out var targetNode))
                {
                    CreateEdge(newNode, targetNode);
                }
            }
        }
    }
}
