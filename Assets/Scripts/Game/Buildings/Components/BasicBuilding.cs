using UnityEngine;

namespace PixelPunk.Buildings.Components
{
    /// <summary>
    /// Basic implementation of a placeable building.
    /// Handles grid-based placement and size requirements.
    /// </summary>
    public class BasicBuilding : MonoBehaviour, IGridPlaceable
    {
        [SerializeField, Tooltip("Size of the building in grid cells (width, height)")]
        private Vector2Int size = Vector2Int.one;

        [SerializeField, Tooltip("Offset from grid cell center for fine-tuning building position")]
        private Vector2 gridOffset = Vector2.zero;

        /// <summary>
        /// Gets the size of the building in grid cells
        /// </summary>
        public Vector2Int Size => size;

        /// <summary>
        /// Gets the offset from grid cell center
        /// </summary>
        public Vector2 GridOffset => gridOffset;

        /// <summary>
        /// Tracks whether the building has been placed on the grid
        /// </summary>
        private bool isPlaced;

        /// <summary>
        /// Validates if the building can be placed at the specified position.
        /// Override this method to add custom placement rules.
        /// </summary>
        /// <param name="position">World position to check for placement.</param>
        /// <returns>True if placement is valid, false otherwise.</returns>
        public virtual bool CanBePlaced(Vector3 position)
        {
            return !isPlaced;
        }

        /// <summary>
        /// Places the building at the specified position.
        /// </summary>
        /// <param name="position">World position where the building should be placed.</param>
        public virtual void Place(Vector3 position)
        {
            // Apply grid offset when placing
            Vector3 offsetPosition = position + new Vector3(gridOffset.x, gridOffset.y, 0);
            transform.position = offsetPosition;
            isPlaced = true;
        }

        /// <summary>
        /// Cancels the placement operation and cleans up if necessary.
        /// </summary>
        public virtual void CancelPlacement()
        {
            if (!isPlaced)
            {
                Destroy(gameObject);
            }
        }
    }
}
