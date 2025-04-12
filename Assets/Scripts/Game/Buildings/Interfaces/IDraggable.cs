using UnityEngine;

namespace PixelPunk.Buildings
{
    /// <summary>
    /// Interface for objects that can be dragged in the game world.
    /// Provides methods for handling drag and drop operations.
    /// </summary>
    public interface IDraggable
    {
        /// <summary>
        /// Initiates the dragging operation.
        /// Should set up any necessary state for dragging.
        /// </summary>
        void StartDragging();

        /// <summary>
        /// Updates the position of the dragged object.
        /// Called continuously while the object is being dragged.
        /// </summary>
        /// <param name="position">The new world position to update to.</param>
        void UpdateDragPosition(Vector3 position);

        /// <summary>
        /// Ends the dragging operation.
        /// Should handle final placement or cancellation.
        /// </summary>
        void StopDragging();

        /// <summary>
        /// Gets whether the object is currently being dragged.
        /// </summary>
        bool IsDragging { get; }
    }
}
