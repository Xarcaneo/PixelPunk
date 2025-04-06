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
    public class MenuFlowGraphView : GraphView, IMenuFlowGraphView
    {
        private MenuFlowEditorWindow editorWindow;
        private Dictionary<string, MenuNode> nodeMap = new Dictionary<string, MenuNode>();
        private bool isClearing = false;
        private PanelTracker panelTracker;

        // Layout values from style system
        private float HorizontalSpacing => MenuFlowGraphViewStyleValues.HorizontalSpacing;
        private float VerticalSpacing => MenuFlowGraphViewStyleValues.VerticalSpacing;
        private float RootOffsetX => MenuFlowGraphViewStyleValues.RootOffsetX;
        private float RootOffsetY => MenuFlowGraphViewStyleValues.RootOffsetY;
        private float NodeWidth => MenuFlowGraphViewStyleValues.NodeWidth;
        private float NodeHeight => MenuFlowGraphViewStyleValues.NodeHeight;

        /// <summary>
        /// Gets the graph view instance.
        /// </summary>
        public GraphView GraphView => this;

        /// <summary>
        /// Gets the dictionary mapping node names to their corresponding menu nodes.
        /// </summary>
        public Dictionary<string, MenuNode> NodeMap => nodeMap;

        /// <summary>
        /// Initializes a new instance of the MenuFlowGraphView class.
        /// </summary>
        /// <param name="window">The editor window instance.</param>
        public MenuFlowGraphView(MenuFlowEditorWindow window)
        {
            editorWindow = window;
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            var grid = new GridBackground();
            grid.AddToClassList("menu-flow-grid");
            Insert(0, grid);
            grid.StretchToParentSize();

            // Add style sheets
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.menuflow.ui/Editor/MenuFlow.uss"));
            AddToClassList("menu-flow-graph");

            // Set up port connections
            SetupZoom(0.1f, ContentZoomer.DefaultMaxScale);
            
            // Initialize panel tracker
            var menuSystem = AssetDatabase.FindAssets("t:MenuSystem").Select(guid => AssetDatabase.LoadAssetAtPath<MenuSystem>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
            panelTracker = new PanelTracker(menuSystem);
            panelTracker.OnPanelChanged += OnPanelChanged;

            // Override connection validation
            graphViewChanged = (changes) =>
            {
                if (isClearing) return changes;

                if (changes.edgesToCreate != null)
                {
                    var edgesToRemove = new List<Edge>();
                    foreach (var edge in changes.edgesToCreate)
                    {
                        var sourceNode = edge.output.node as MenuNode;
                        var targetNode = edge.input.node as MenuNode;
                        
                        if (WouldCreateCycle(sourceNode, targetNode))
                        {
                            edgesToRemove.Add(edge);
                            Debug.LogWarning("Cannot create connection: would create a cycle");
                        }
                        else
                        {
                            targetNode.Menu.parentMenu = sourceNode.Menu;
                            if (!sourceNode.Menu.childMenus.Contains(targetNode.Menu))
                            {
                                sourceNode.Menu.childMenus.Add(targetNode.Menu);
                            }
                            EditorUtility.SetDirty(sourceNode.Menu);
                            EditorUtility.SetDirty(targetNode.Menu);
                        }
                    }

                    foreach (var edge in edgesToRemove)
                    {
                        changes.edgesToCreate.Remove(edge);
                    }
                }

                if (changes.elementsToRemove != null && !isClearing)
                {
                    foreach (var element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            var sourceNode = edge.output.node as MenuNode;
                            var targetNode = edge.input.node as MenuNode;
                            
                            if (targetNode.Menu.parentMenu == sourceNode.Menu)
                            {
                                targetNode.Menu.parentMenu = null;
                            }
                            sourceNode.Menu.childMenus.Remove(targetNode.Menu);
                            
                            EditorUtility.SetDirty(sourceNode.Menu);
                            EditorUtility.SetDirty(targetNode.Menu);
                        }
                    }
                }

                // Save positions when nodes are moved
                if (changes.movedElements != null)
                {
                    foreach (var element in changes.movedElements)
                    {
                        if (element is MenuNode node)
                        {
                            var rect = node.GetPosition();
                            node.Menu.SetNodePosition(node.Menu.name, rect.position);
                            EditorUtility.SetDirty(node.Menu);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }

                return changes;
            };
        }

        /// <summary>
        /// Handles the panel changed event.
        /// Updates node highlighting based on the current panel.
        /// </summary>
        private void OnPanelChanged()
        {
            // Update node highlighting
            var currentPanel = panelTracker.CurrentPanel;
            foreach (var node in nodeMap.Values)
            {
                node.SetActive(node.Menu == currentPanel);
            }
        }

        /// <summary>
        /// Clears the graph view by removing all nodes and edges.
        /// </summary>
        public void ClearGraph()
        {
            isClearing = true;
            foreach (var edge in edges.ToList())
            {
                RemoveElement(edge);
            }
            foreach (var node in nodes.ToList())
            {
                RemoveElement(node);
            }
            nodeMap.Clear();
            isClearing = false;
        }

        /// <summary>
        /// Adds a new node to the graph view.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddNode(MenuNode node)
        {
            // Restore saved position
            var savedPos = node.Menu.GetNodePosition(node.Menu.name);
            if (savedPos != Vector2.zero)
            {
                node.SetPosition(new Rect(savedPos, new Vector2(NodeWidth, NodeHeight)));
            }

            GraphView.AddElement(node);
            nodeMap[node.Menu.name] = node;
            
            // Update node highlighting if it's the active panel
            if (panelTracker.CurrentPanel == node.Menu)
            {
                node.SetActive(true);
            }
        }

        /// <summary>
        /// Builds the contextual menu for the graph view.
        /// </summary>
        /// <param name="evt">The contextual menu populate event.</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Sort/Horizontally", (a) => SortNodes(true));
            evt.menu.AppendAction("Sort/Vertically", (a) => SortNodes(false));
        }

        /// <summary>
        /// Sorts the nodes in the graph view.
        /// </summary>
        /// <param name="horizontal">True to sort horizontally; otherwise false.</param>
        public void SortNodes(bool horizontal)
        {
            if (nodeMap.Count == 0) return;

            // Find root nodes (nodes without parents)
            var rootNodes = nodeMap.Values.Where(n => n.Menu.ParentMenu == null)
                .OrderBy(n => n.Menu.name)
                .ToList();

            if (rootNodes.Count == 0) return;

            var currentRootPos = new Vector2(RootOffsetX, RootOffsetY);
            var processedNodes = new HashSet<string>();
            var maxPositions = new Dictionary<int, Vector2>(); // Track max positions per level
            
            foreach (var rootNode in rootNodes)
            {
                var maxPos = PositionNodeHierarchy(rootNode, currentRootPos, horizontal, processedNodes, 0, maxPositions);
                
                // Update position for next root node
                if (horizontal)
                {
                    currentRootPos = new Vector2(currentRootPos.x, maxPos.y + VerticalSpacing);
                }
                else
                {
                    // In vertical mode, ensure we start after the widest node in the previous tree
                    var maxX = maxPositions.Values.Max(p => p.x);
                    currentRootPos = new Vector2(maxX + HorizontalSpacing, RootOffsetY);
                }
            }

            // Save all node positions and mark assets as dirty
            foreach (var node in nodeMap.Values)
            {
                var rect = node.GetPosition();
                node.Menu.SetNodePosition(node.Menu.name, rect.position);
                EditorUtility.SetDirty(node.Menu);
            }
            AssetDatabase.SaveAssets();

            // Frame all nodes
            FrameAll();
        }

        /// <summary>
        /// Positions a node hierarchy in the graph view.
        /// </summary>
        /// <param name="node">The node to position.</param>
        /// <param name="position">The initial position of the node.</param>
        /// <param name="horizontal">True to position horizontally; otherwise false.</param>
        /// <param name="processedNodes">A set of processed node names.</param>
        /// <param name="level">The current level in the hierarchy.</param>
        /// <param name="maxPositions">A dictionary of maximum positions per level.</param>
        /// <returns>The maximum position of the node hierarchy.</returns>
        private Vector2 PositionNodeHierarchy(MenuNode node, Vector2 position, bool horizontal, 
            HashSet<string> processedNodes, int level, Dictionary<int, Vector2> maxPositions)
        {
            if (node == null || !processedNodes.Add(node.Menu.name))
                return position;

            // Get max position for this level
            if (!maxPositions.ContainsKey(level))
            {
                maxPositions[level] = position;
            }

            // In vertical mode, ensure we don't overlap with previous nodes at this level
            var nodePos = horizontal ? position : new Vector2(maxPositions[level].x, position.y);
            var nodeRect = new Rect(nodePos, new Vector2(NodeWidth, NodeHeight));
            node.SetPosition(nodeRect);
            var rect = node.GetPosition();
            node.Menu.SetNodePosition(node.Menu.name, rect.position);
            EditorUtility.SetDirty(node.Menu);

            // Update max position for this level
            maxPositions[level] = new Vector2(
                Mathf.Max(maxPositions[level].x + NodeWidth + HorizontalSpacing, nodePos.x + NodeWidth + HorizontalSpacing),
                Mathf.Max(maxPositions[level].y + NodeHeight + VerticalSpacing, nodePos.y + NodeHeight + VerticalSpacing)
            );

            // Get child nodes sorted by name for consistent layout
            var childNodes = node.Menu.childMenus
                .Select(child => nodeMap.TryGetValue(child.name, out var n) ? n : null)
                .Where(n => n != null)
                .OrderBy(n => n.Menu.name)
                .ToList();

            if (childNodes.Count == 0)
            {
                return horizontal 
                    ? new Vector2(nodePos.x + NodeWidth, nodePos.y + NodeHeight)
                    : new Vector2(nodePos.x + NodeWidth + HorizontalSpacing, nodePos.y + NodeHeight);
            }

            var maxPos = nodePos;
            var baseChildPos = horizontal
                ? new Vector2(nodePos.x + NodeWidth + HorizontalSpacing, nodePos.y)
                : new Vector2(nodePos.x, nodePos.y + NodeHeight + VerticalSpacing);
            var childStartPos = baseChildPos;

            foreach (var childNode in childNodes)
            {
                var childPos = PositionNodeHierarchy(childNode, childStartPos, horizontal, processedNodes, level + 1, maxPositions);
                
                // Update maximum positions
                maxPos = new Vector2(
                    Mathf.Max(maxPos.x, childPos.x),
                    Mathf.Max(maxPos.y, childPos.y)
                );

                // Update start position for next sibling
                if (horizontal)
                {
                    childStartPos = new Vector2(childStartPos.x, childPos.y + VerticalSpacing);
                }
                else
                {
                    // In vertical mode, move right from the base child position
                    childStartPos = new Vector2(baseChildPos.x + (childNodes.IndexOf(childNode) + 1) * (NodeWidth + HorizontalSpacing), baseChildPos.y);
                }
            }

            return maxPos;
        }

        /// <summary>
        /// Gets a list of ports that are compatible for connection with the given start port.
        /// Ensures connections only occur between input and output ports and prevents cycles.
        /// </summary>
        /// <param name="startPort">The port to check compatibility with</param>
        /// <param name="nodeAdapter">Node adapter for compatibility checking</param>
        /// <returns>List of compatible ports</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => 
                endPort.direction != startPort.direction && 
                endPort.node != startPort.node &&
                !WouldCreateCycle(startPort.node as MenuNode, endPort.node as MenuNode)
            ).ToList();
        }

        /// <summary>
        /// Checks if creating an edge between two nodes would create a cycle in the menu hierarchy.
        /// Traverses up the parent chain from the source node to detect potential cycles.
        /// </summary>
        /// <param name="sourceNode">The source node of the potential edge</param>
        /// <param name="targetNode">The target node of the potential edge</param>
        /// <returns>True if a cycle would be created; otherwise false</returns>
        private bool WouldCreateCycle(MenuNode sourceNode, MenuNode targetNode)
        {
            if (sourceNode == null || targetNode == null)
                return false;

            // Check if target is already a parent of source (would create cycle)
            var current = sourceNode;
            while (current != null && current.Menu.ParentMenu != null)
            {
                if (current.Menu.ParentMenu == targetNode.Menu)
                    return true;
                current = nodeMap.TryGetValue(current.Menu.ParentMenu.name, out var parentNode) ? parentNode : null;
            }
            return false;
        }

        /// <summary>
        /// Overrides the delete selection behavior to only allow deletion of edges, not nodes.
        /// This prevents accidental deletion of menu nodes while still allowing connection management.
        /// </summary>
        /// <returns>Event propagation status</returns>
        public override EventPropagation DeleteSelection()
        {
            // Only allow deletion of edges, not nodes
            var edgesToDelete = selection.OfType<Edge>().ToList();
            if (edgesToDelete.Any())
            {
                foreach (var edge in edgesToDelete)
                {
                    RemoveElement(edge);
                }
            }
            return EventPropagation.Stop;
        }
    }
}
