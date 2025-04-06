using UnityEngine;
using UnityEditor.Experimental.GraphView;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Interface for menu nodes in the graph view.
    /// Defines core functionality for menu panel visualization and interaction.
    /// </summary>
    public interface IMenuNode
    {
        /// <summary>
        /// Gets the menu definition associated with this node
        /// </summary>
        MenuDefinition Menu { get; }

        /// <summary>
        /// Gets the input port for incoming connections
        /// </summary>
        Port InputPort { get; }

        /// <summary>
        /// Gets the output port for outgoing connections
        /// </summary>
        Port OutputPort { get; }

        /// <summary>
        /// Sets the active state of the node
        /// </summary>
        /// <param name="active">True to set the node as active; false otherwise</param>
        void SetActive(bool active);

        /// <summary>
        /// Sets the position of the node in the graph
        /// </summary>
        /// <param name="position">The new position for the node</param>
        void SetPosition(Rect position);
    }
}
