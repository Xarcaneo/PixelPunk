using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using MenuFlow.ScriptableObjects;
using System;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Represents a node in the MenuFlow graph that visualizes a menu panel.
    /// Handles port connections, visual styling, and interaction capabilities.
    /// </summary>
    public class MenuNode : Node, IMenuNode
    {
        private MenuDefinition menu;
        private IMenuFlowGraphView graphView;
        private Port inputPort;
        private Port outputPort;
        private VisualElement titleSection;

        /// <summary>
        /// Gets the menu definition associated with this node
        /// </summary>
        public MenuDefinition Menu => menu;

        /// <summary>
        /// Gets the input port for incoming connections
        /// </summary>
        public Port InputPort => inputPort;

        /// <summary>
        /// Gets the output port for outgoing connections
        /// </summary>
        public Port OutputPort => outputPort;

        /// <summary>
        /// Initializes a new instance of the MenuNode class.
        /// Sets up the node's visual elements, ports, and interaction capabilities.
        /// </summary>
        /// <param name="menuDef">The menu definition to represent</param>
        /// <param name="position">Initial position of the node</param>
        /// <param name="graphView">The parent graph view</param>
        public MenuNode(MenuDefinition menuDef, Vector2 position, IMenuFlowGraphView graphView)
        {
            menu = menuDef;
            this.graphView = graphView;

            // Set initial position
            var rect = new Rect(position, new Vector2(150, 120));
            SetPosition(rect);
            
            // Ensure position is saved in menu definition
            menu.SetNodePosition(menu.name, position);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(menu);
            #endif

            // Remove default title
            titleContainer.RemoveFromHierarchy();

            // Create top section with input port
            var topSection = new VisualElement();
            topSection.AddToClassList("menu-node__port-section");
            inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.AddToClassList("menu-node__port");
            topSection.Add(inputPort);
            mainContainer.Add(topSection);

            // Create title section
            titleSection = new VisualElement();
            titleSection.AddToClassList("menu-node__title-section");
            
            var titleLabel = new Label(menuDef.name);
            titleLabel.AddToClassList("menu-node__title-label");
            titleSection.Add(titleLabel);
            mainContainer.Add(titleSection);

            // Create bottom section with output port
            var bottomSection = new VisualElement();
            bottomSection.AddToClassList("menu-node__port-section");
            outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "";
            outputPort.AddToClassList("menu-node__port");
            bottomSection.Add(outputPort);
            mainContainer.Add(bottomSection);

            // Add custom class for styling
            AddToClassList("menu-node");
            mainContainer.AddToClassList("menu-node__container");

            // Disable collapsing since we don't need it
            expanded = true;
            capabilities &= ~Capabilities.Collapsible;
            
            // Remove copy, duplicate, and delete capabilities
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Movable;
            
            // Re-enable just the movable capability since we want to be able to drag nodes
            capabilities |= Capabilities.Movable;

            capabilities |= Capabilities.Movable | Capabilities.Deletable | Capabilities.Selectable;
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>
        /// Sets the active state of the node, updating its visual appearance
        /// </summary>
        /// <param name="active">True to set the node as active; false otherwise</param>
        public void SetActive(bool active)
        {
            if (active)
                AddToClassList("menu-node--active");
            else
                RemoveFromClassList("menu-node--active");
        }

        /// <summary>
        /// Overrides the default context menu behavior to prevent unwanted operations
        /// </summary>
        /// <param name="evt">The context menu event</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Override to prevent the default context menu from being built
            evt.StopPropagation();
        }
    }
}
