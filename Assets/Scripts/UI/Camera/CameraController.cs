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
        cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraController] No camera found in scene!");
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

        // Handle input based on platform
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        
        switch (touch.phase)
        {
            case TouchPhase.Began:
                lastMousePosition = touch.position;
                isDragging = true;
                break;

            case TouchPhase.Moved:
                if (isDragging)
                {
                    Vector2 delta = touch.position - lastMousePosition;
                    MoveCamera(delta);
                    lastMousePosition = touch.position;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                break;
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            MoveCamera(delta);
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
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
