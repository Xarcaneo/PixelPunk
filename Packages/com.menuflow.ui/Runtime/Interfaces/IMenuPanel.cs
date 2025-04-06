using System.Threading.Tasks;
using UnityEngine;

namespace MenuFlow.Interfaces
{
    /// <summary>
    /// Interface for menu panels that can be shown and hidden with transitions
    /// </summary>
    public interface IMenuPanel
    {
        /// <summary>
        /// The menu definition this panel represents
        /// </summary>
        MenuFlow.ScriptableObjects.MenuDefinition MenuDefinition { get; }
        
        /// <summary>
        /// Shows the menu panel with an optional transition
        /// </summary>
        /// <returns>Task that completes when the transition is finished</returns>
        Task Show();
        
        /// <summary>
        /// Exits the menu panel and its parent hierarchy with an optional transition
        /// </summary>
        /// <returns>Task that completes when the exit transition is finished</returns>
        Task Exit();
        
        /// <summary>
        /// Immediately shows or hides the panel without transition
        /// </summary>
        void SetVisible(bool visible);
        
        /// <summary>
        /// Gets the GameObject representing this panel
        /// </summary>
        GameObject gameObject { get; }
    }
}
