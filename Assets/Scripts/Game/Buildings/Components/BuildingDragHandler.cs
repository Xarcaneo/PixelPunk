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
        /// Initiates the dragging operation after hold threshold is met.
        /// </summary>
        public void StartDragging()
        {
            if (!enabled || mainCamera == null || buildingSystem == null) return;

            isDragging = true;
            isHolding = false;
            Vector3 mousePosition = GetMouseWorldPosition();
            offset = transform.position - mousePosition;

            // Notify camera that we're starting to drag
            cameraController?.NotifyBuildingDragStarted(this);
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
