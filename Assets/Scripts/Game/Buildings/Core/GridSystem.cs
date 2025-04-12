using UnityEngine;

namespace PixelPunk.Buildings.Core
{
    /// <summary>
    /// Handles grid-based operations and coordinates for the building system.
    /// Provides methods for converting between world and grid coordinates.
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        [SerializeField, Tooltip("The Unity Grid component used for coordinate conversion")]
        private Grid grid;

        /// <summary>
        /// Snaps a world position to the nearest grid cell.
        /// </summary>
        /// <param name="worldPosition">The world position to snap.</param>
        /// <returns>The snapped world position aligned to the grid.</returns>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector3Int cellPosition = grid.WorldToCell(worldPosition);
            return grid.CellToWorld(cellPosition);
        }

        /// <summary>
        /// Converts a world position to its corresponding grid cell coordinates.
        /// </summary>
        /// <param name="worldPosition">The world position to convert.</param>
        /// <returns>The grid cell coordinates (Vector3Int).</returns>
        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return grid.WorldToCell(worldPosition);
        }

        /// <summary>
        /// Converts grid cell coordinates to world position.
        /// </summary>
        /// <param name="cellPosition">The grid cell coordinates to convert.</param>
        /// <returns>The world position at the center of the specified grid cell.</returns>
        public Vector3 CellToWorld(Vector3Int cellPosition)
        {
            return grid.CellToWorld(cellPosition);
        }
    }
}
