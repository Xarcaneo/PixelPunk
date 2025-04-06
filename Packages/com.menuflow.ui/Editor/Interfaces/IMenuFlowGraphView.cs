using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Interface for the MenuFlow graph view component.
    /// Defines core functionality for managing nodes and graph layout.
    /// </summary>
    public interface IMenuFlowGraphView
    {
        /// <summary>
        /// Adds a node to the graph view
        /// </summary>
        /// <param name="node">The menu node to add</param>
        void AddNode(MenuNode node);

        /// <summary>
        /// Clears all nodes and edges from the graph
        /// </summary>
        void ClearGraph();

        /// <summary>
        /// Sorts nodes in the graph either horizontally or vertically
        /// </summary>
        /// <param name="horizontal">If true, sort horizontally; otherwise sort vertically</param>
        void SortNodes(bool horizontal);

        /// <summary>
        /// Gets the underlying Unity GraphView component
        /// </summary>
        GraphView GraphView { get; }

        /// <summary>
        /// Gets the mapping of menu names to their corresponding nodes
        /// </summary>
        Dictionary<string, MenuNode> NodeMap { get; }
    }
}
