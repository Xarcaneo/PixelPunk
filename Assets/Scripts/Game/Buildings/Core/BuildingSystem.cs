using UnityEngine;
using System.Collections.Generic;

namespace PixelPunk.Buildings.Core
{
    /// <summary>
    /// Core system for managing building placement and validation.
    /// Handles grid-based placement, collision checking, and building management.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the GridSystem component for coordinate conversion")]
        private GridSystem gridSystem;
        
        [SerializeField, Tooltip("Parent transform for organizing placed buildings")]
        private Transform buildingsParent;

        /// <summary>
        /// Dictionary tracking placed buildings by their grid position
        /// </summary>
        private readonly Dictionary<Vector3Int, IGridPlaceable> placedBuildings = new Dictionary<Vector3Int, IGridPlaceable>();

        /// <summary>
        /// Checks if a building can be placed at the specified world position.
        /// </summary>
        /// <param name="worldPosition">The world position to check.</param>
        /// <param name="building">The building to place.</param>
        /// <returns>True if placement is valid, false otherwise.</returns>
        public bool CanPlaceAt(Vector3 worldPosition, IGridPlaceable building)
        {
            Vector3Int cellPosition = gridSystem.WorldToCell(worldPosition);
            return !placedBuildings.ContainsKey(cellPosition) && building.CanBePlaced(worldPosition);
        }

        /// <summary>
        /// Attempts to place a building at the specified world position.
        /// </summary>
        /// <param name="worldPosition">The world position for placement.</param>
        /// <param name="building">The building to place.</param>
        /// <returns>True if placement was successful, false otherwise.</returns>
        public bool PlaceBuilding(Vector3 worldPosition, IGridPlaceable building)
        {
            Vector3Int cellPosition = gridSystem.WorldToCell(worldPosition);
            
            if (!CanPlaceAt(worldPosition, building))
            {
                return false;
            }

            building.Place(gridSystem.CellToWorld(cellPosition));
            placedBuildings[cellPosition] = building;
            return true;
        }

        /// <summary>
        /// Gets the grid-aligned position for the given world position.
        /// </summary>
        /// <param name="worldPosition">The world position to snap to grid.</param>
        /// <returns>The snapped position on the grid.</returns>
        public Vector3 GetSnappedPosition(Vector3 worldPosition)
        {
            return gridSystem.SnapToGrid(worldPosition);
        }
    }
}
