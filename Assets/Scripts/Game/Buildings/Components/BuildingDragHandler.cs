using UnityEngine;
using PixelPunk.Buildings.Core;

namespace PixelPunk.Buildings.Components
{
    /// <summary>
    /// Handles drag and drop behavior for buildings.
    /// Manages hold-to-drag functionality and grid snapping.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BuildingDragHandler : MonoBehaviour, IDraggable
    {
        [SerializeField, Tooltip("Time in seconds required to hold before dragging starts")]
        private float holdTimeThreshold = 1f;
        
        [SerializeField, Tooltip("Reference to the building system. Will find one if not set")]
        private BuildingSystem buildingSystem;
        
        private bool isHolding;
        private float holdTime;
        private bool isDragging;
        private Camera mainCamera;
        private Vector3 offset;
        private IGridPlaceable placeable;

        /// <summary>
        /// Gets whether the building is currently being dragged.
        /// </summary>
        public bool IsDragging => isDragging;

        private void Start()
        {
            mainCamera = Camera.main;
            if (buildingSystem == null)
            {
                buildingSystem = FindFirstObjectByType<BuildingSystem>();
            }
            placeable = GetComponent<IGridPlaceable>();
        }
        
        private void Update()
        {
            if (isDragging)
            {
                UpdateDragPosition(GetMouseWorldPosition());

                if (Input.GetMouseButtonUp(0))
                {
                    StopDragging();
                }
            }
            else if (isHolding)
            {
                // Track hold time
                holdTime += Time.deltaTime;
                
                if (holdTime >= holdTimeThreshold)
                {
                    StartDragging();
                }
                
                if (Input.GetMouseButtonUp(0))
                {
                    isHolding = false;
                    holdTime = 0f;
                }
            }
        }

        private void OnMouseDown()
        {
            isHolding = true;
            holdTime = 0f;
        }

        /// <summary>
        /// Initiates the dragging operation after hold threshold is met.
        /// </summary>
        public void StartDragging()
        {
            isDragging = true;
            isHolding = false;
            Vector3 mousePosition = GetMouseWorldPosition();
            offset = transform.position - mousePosition;
        }

        /// <summary>
        /// Updates the building position while dragging, snapping to grid.
        /// </summary>
        /// <param name="position">Current mouse/touch position in world space.</param>
        public void UpdateDragPosition(Vector3 position)
        {
            Vector3 targetPosition = position + offset;
            Vector3 snappedPosition = buildingSystem.GetSnappedPosition(targetPosition);
            transform.position = snappedPosition;
        }

        /// <summary>
        /// Attempts to place the building at its current position.
        /// </summary>
        public void StopDragging()
        {
            if (placeable != null && buildingSystem.CanPlaceAt(transform.position, placeable))
            {
                buildingSystem.PlaceBuilding(transform.position, placeable);
                isDragging = false;
            }
        }

        /// <summary>
        /// Converts screen position to world position for the current building height.
        /// </summary>
        /// <returns>World position at the mouse/touch point.</returns>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }
    }
}
