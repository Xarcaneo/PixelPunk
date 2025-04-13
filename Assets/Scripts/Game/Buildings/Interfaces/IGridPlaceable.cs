using UnityEngine;

namespace PixelPunk.Buildings
{
    /// <summary>
    /// Interface for objects that can be placed on a grid system.
    /// Implements placement validation and handling.
    /// </summary>
    public interface IGridPlaceable
    {
        /// <summary>
        /// Gets the size of the object in grid cells (width, height)
        /// </summary>
        Vector2Int Size { get; }

        /// <summary>
        /// Validates if the object can be placed at the specified position.
        /// </summary>
        /// <param name="position">World position to check for placement.</param>
        /// <returns>True if the object can be placed at the position, false otherwise.</returns>
        bool CanBePlaced(Vector3 position);

        /// <summary>
        /// Places the object at the specified position.
        /// </summary>
        /// <param name="position">World position where the object should be placed.</param>
        void Place(Vector3 position);

        /// <summary>
        /// Cancels the current placement operation.
        /// Usually called when placement validation fails or user cancels placement.
        /// </summary>
        void CancelPlacement();
    }
}
