#nullable enable

using UnityEngine;
using PixelPunk.Buildings.Core;
using System;

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
        private BuildingSystem? buildingSystem;

        /// <summary>
        /// Invoked when the building is clicked but not held long enough to drag
        /// </summary>
        public event Action<BuildingDragHandler>? OnQuickClick;
        
        private bool isHolding;
        private float holdTime;
        private bool isDragging;
        private Camera? mainCamera;
        private Vector3 offset;
        private IGridPlaceable? placeable;
        private CameraController? cameraController;

        /// <summary>
        /// Gets whether the building is currently being dragged.
        /// </summary>
        public bool IsDragging => isDragging;

        private void Start()
        {
            mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
            if (mainCamera == null)
            {
                Debug.LogError("[BuildingDragHandler] No camera found in scene!");
                enabled = false;
                return;
            }

            buildingSystem ??= FindFirstObjectByType<BuildingSystem>();
            if (buildingSystem == null)
            {
                Debug.LogError("[BuildingDragHandler] No BuildingSystem found in scene!");
                enabled = false;
                return;
            }

            placeable = GetComponent<IGridPlaceable>();
            if (placeable == null)
            {
                Debug.LogError("[BuildingDragHandler] No IGridPlaceable component found!");
                enabled = false;
                return;
            }

            cameraController = mainCamera.GetComponent<CameraController>();
        }
        
        private void Update()
        {
            if (!enabled) return;

            if (isDragging && mainCamera != null)
            {
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = transform.position.z;
                
                Vector3 targetPosition = mousePosition + offset;
                
                // Snap to grid
                if (buildingSystem != null)
                {
                    Vector3 preSnapPos = targetPosition;
                    targetPosition = buildingSystem.GridSystem.SnapToGrid(targetPosition);
                    
                    // Update placement overlay
                    if (placeable != null)
                    {
                        bool isValidPlacement = buildingSystem.CanPlaceAt(targetPosition, placeable);
                        buildingSystem.GridSystem.ShowPlacementOverlay(targetPosition, placeable.Size, isValidPlacement);

                        // Apply grid offset during drag
                        if (placeable is BasicBuilding building)
                        {
                            Vector3 beforeOffset = targetPosition;
                            targetPosition += new Vector3(building.GridOffset.x, building.GridOffset.y, 0);
                        }
                    }
                }
                
                transform.position = targetPosition;

                // Disable camera panning while dragging
                if (cameraController != null)
                {
                    cameraController.enabled = false;
                }

                // Check for mouse up to end dragging
                if (Input.GetMouseButtonUp(0))
                {
                    OnEndDrag();
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
                    // If released before threshold, trigger quick click
                    if (holdTime < holdTimeThreshold)
                    {
                        OnQuickClick?.Invoke(this);
                    }
                    isHolding = false;
                    holdTime = 0f;
                }
            }
        }

        private void OnMouseDown()
        {
            if (!enabled) return;

            isHolding = true;
            holdTime = 0f;
        }

        /// <summary>
        /// Called when dragging should begin. Initializes drag state and calculates initial offset.
        /// </summary>
        public void StartDragging()
        {
            if (!enabled || mainCamera == null || buildingSystem == null) return;

            isDragging = true;
            isHolding = false;

            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;

            // Calculate offset based on grid snapping
            if (buildingSystem != null && placeable != null)
            {
                Vector3 snappedMousePos = buildingSystem.GridSystem.SnapToGrid(mousePosition);
                if (placeable is BasicBuilding building)
                {
                    snappedMousePos += new Vector3(building.GridOffset.x, building.GridOffset.y, 0);
                }
                offset = transform.position - snappedMousePos;
            }
            else
            {
                offset = transform.position - mousePosition;
            }

            
            // Notify camera controller
            if (cameraController != null)
            {
                cameraController.enabled = false;
            }
        }

        public void OnBeginDrag()
        {
            if (!isHolding) return;
            
            isDragging = true;
            offset = transform.position - mainCamera!.ScreenToWorldPoint(Input.mousePosition);
            offset.z = 0;
            
            // Get the IGridPlaceable component if not already cached
            placeable ??= GetComponent<IGridPlaceable>();
            
            // Cache camera controller reference
            cameraController ??= FindFirstObjectByType<CameraController>();
        }

        public void OnEndDrag()
        {
            if (!isDragging) return;
            
            isDragging = false;
            isHolding = false;
            holdTime = 0f;
            
            // Clear placement overlay
            if (buildingSystem != null)
            {
                buildingSystem.GridSystem.ClearOverlay();
            }

            // Re-enable camera panning
            if (cameraController != null)
            {
                cameraController.enabled = true;
            }
        }

        /// <summary>
        /// Updates the building position while dragging, snapping to grid.
        /// </summary>
        /// <param name="position">Current mouse/touch position in world space.</param>
        public void UpdateDragPosition(Vector3 position)
        {
            if (!enabled || buildingSystem == null) return;

            Vector3 targetPosition = position + offset;
            Vector3 snappedPosition = buildingSystem.GetSnappedPosition(targetPosition);
            transform.position = snappedPosition;
        }

        /// <summary>
        /// Attempts to place the building at its current position.
        /// </summary>
        public void StopDragging()
        {
            if (placeable != null && buildingSystem != null && buildingSystem.CanPlaceAt(transform.position, placeable))
            {
                buildingSystem.PlaceBuilding(transform.position, placeable);
            }
            isDragging = false;

            // Notify camera that we're done dragging
            cameraController?.NotifyBuildingDragStopped(this);
        }
 
        /// <summary>
        /// Converts screen position to world position for the current building height.
        /// </summary>
        /// <returns>World position at the mouse/touch point.</returns>
        private Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null) return Vector3.zero;

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePosition);
        }
    }
}
