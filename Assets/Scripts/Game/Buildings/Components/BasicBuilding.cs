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
            // Add your placement validation logic here
            // For example:
            // - Check if there's enough space for the building size
            // - Check resource requirements
            // - Check building restrictions
            return true;
        }

        /// <summary>
        /// Places the building at the specified position.
        /// </summary>
        /// <param name="position">World position where the building should be placed.</param>
        public virtual void Place(Vector3 position)
        {
            transform.position = position;
            isPlaced = true;
            // Add any placement effects or initialization here
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
