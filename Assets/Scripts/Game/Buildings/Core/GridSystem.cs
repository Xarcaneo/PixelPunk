using UnityEngine;
using UnityEngine.Tilemaps;

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

        [SerializeField, Tooltip("Tilemap for showing valid/invalid placement overlay")]
        private Tilemap overlayTilemap;

        [SerializeField, Tooltip("Tile to show valid placement")]
        private TileBase validPlacementTile;

        [SerializeField, Tooltip("Tile to show invalid placement")]
        private TileBase invalidPlacementTile;

        private void Awake()
        {
            if (overlayTilemap == null)
            {
                var overlayGo = new GameObject("PlacementOverlay");
                overlayGo.transform.SetParent(transform);
                overlayTilemap = overlayGo.AddComponent<Tilemap>();
                var renderer = overlayGo.AddComponent<TilemapRenderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        /// <summary>
        /// Shows placement overlay at the specified position with the given size.
        /// </summary>
        /// <param name="worldPosition">Center position of the building</param>
        /// <param name="size">Size of the building in grid cells</param>
        /// <param name="isValid">Whether the placement is valid</param>
        public void ShowPlacementOverlay(Vector3 worldPosition, Vector2Int size, bool isValid)
        {
            Vector3Int originCell = grid.WorldToCell(worldPosition);
            Vector3Int offset = new Vector3Int(-(size.x / 2), -(size.y / 2), 0);
            
            ClearOverlay();
            
            TileBase tileToUse = isValid ? validPlacementTile : invalidPlacementTile;
            
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int pos = originCell + offset + new Vector3Int(x, y, 0);
                    overlayTilemap.SetTile(pos, tileToUse);
                }
            }
        }

        /// <summary>
        /// Clears all overlay tiles.
        /// </summary>
        public void ClearOverlay()
        {
            overlayTilemap.ClearAllTiles();
        }

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
