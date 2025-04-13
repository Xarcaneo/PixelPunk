#nullable enable

using UnityEngine;
using PixelPunk.Buildings.Components;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private bool invertDrag = false;

    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY = 50f;

    private Camera? cam;
    private Vector2 lastMousePosition;
    private bool isDragging = false;
    private BuildingDragHandler? activeBuilding;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[CameraController] No main camera found!");
            enabled = false;
            return;
        }
    }

    private bool CanHandleInput()
    {
        // Don't handle input if a building is being dragged
        return activeBuilding == null || !activeBuilding.IsDragging;
    }

    private void Update()
    {
        if (!CanHandleInput() || cam == null) return;

        // Handle mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    lastMousePosition = touch.position;
                    break;
                    
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 currentPos = touch.position;
                        Vector2 difference = currentPos - lastMousePosition;
                        MoveCamera(difference);
                        lastMousePosition = currentPos;
                    }
                    break;
                    
                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
        // Handle mouse input
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 difference = currentPos - lastMousePosition;
                MoveCamera(difference);
                lastMousePosition = currentPos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    private void MoveCamera(Vector2 screenDelta)
    {
        if (cam == null) return;

        // Convert screen movement to world space movement
        float verticalSize = cam.orthographicSize * 2f;
        float horizontalSize = verticalSize * cam.aspect;
        
        Vector3 movement = new Vector3(
            -screenDelta.x * horizontalSize / Screen.width,
            -screenDelta.y * verticalSize / Screen.height,
            0
        ) * (invertDrag ? -1 : 1);

        // Apply movement speed
        movement *= moveSpeed;

        Vector3 targetPosition = transform.position + movement;

        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        transform.position = targetPosition;
    }

    /// <summary>
    /// Called by BuildingDragHandler when a building starts being dragged
    /// </summary>
    public void NotifyBuildingDragStarted(BuildingDragHandler handler)
    {
        activeBuilding = handler;
    }

    /// <summary>
    /// Called by BuildingDragHandler when a building stops being dragged
    /// </summary>
    public void NotifyBuildingDragStopped(BuildingDragHandler handler)
    {
        if (activeBuilding == handler)
        {
            activeBuilding = null;
        }
    }
}
