using UnityEngine;
using UnityEngine.InputSystem;

public class PropInspector1 : MonoBehaviour
{
    private enum InspectState { Idle, Inspecting, Returning }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform inspectPoint;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField] private float reachDistance = 3f;
    [SerializeField] private LayerMask inspectableLayer;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 0.5f;

    private InspectState currentState = InspectState.Idle;
    private GameObject currentProp;
    private Rigidbody propRb;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform originalParent;
    

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // Left Click to pick up or start returning
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentState == InspectState.Idle)
                TryPickUp();
            else if (currentState == InspectState.Inspecting)
                StartPutDown();
        }

        if (currentProp == null) return;

        if (currentState == InspectState.Inspecting)
        {
            // Smoothly move prop to the inspection point
            currentProp.transform.position = Vector3.Lerp(
                currentProp.transform.position,
                inspectPoint.position,
                Time.deltaTime * moveSpeed
            );

            // Rotate prop freely with mouse movement
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float rotX = mouseDelta.x * rotationSpeed;
            float rotY = mouseDelta.y * rotationSpeed;

            currentProp.transform.Rotate(playerCamera.transform.up, -rotX, Space.World);
            currentProp.transform.Rotate(playerCamera.transform.right, rotY, Space.World);
        }
        else if (currentState == InspectState.Returning)
        {
            // Smoothly move and rotate prop back to its original transform
            currentProp.transform.position = Vector3.Lerp(
                currentProp.transform.position,
                originalPos,
                Time.deltaTime * moveSpeed
            );

            currentProp.transform.rotation = Quaternion.Slerp(
                currentProp.transform.rotation,
                originalRot,
                Time.deltaTime * moveSpeed
            );

            // Snap and finish once it is close enough to its original spot
            if (Vector3.Distance(currentProp.transform.position, originalPos) < 0.01f &&
                Quaternion.Angle(currentProp.transform.rotation, originalRot) < 1f)
            {
                FinishPutDown();
            }
        }
    }

    private void TryPickUp()
    {

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, inspectableLayer))
        {
            canvas.gameObject.SetActive(false);
            currentProp = hit.collider.gameObject;
            propRb = currentProp.GetComponent<Rigidbody>();

            originalPos = currentProp.transform.position;
            originalRot = currentProp.transform.rotation;
            originalParent = currentProp.transform.parent;

            if (propRb != null)
            {
                propRb.isKinematic = true;
                propRb.detectCollisions = false;
            }

            currentProp.transform.SetParent(inspectPoint);
            currentState = InspectState.Inspecting;

            if (playerMovementScript != null)
                playerMovementScript.enabled = false;
        }
    }

    private void StartPutDown()
    {
        // Unparent immediately so moving the camera won't drag the returning prop
        currentProp.transform.SetParent(originalParent);
        currentState = InspectState.Returning;

        // Resume player movement while the prop glides back
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
        canvas.gameObject.SetActive(true);
    }

    private void FinishPutDown()
    {
        // Snap exact final values and re-enable physics
        currentProp.transform.position = originalPos;
        currentProp.transform.rotation = originalRot;

        if (propRb != null)
        {
            propRb.isKinematic = false;
            propRb.detectCollisions = true;
        }

        currentProp = null;
        currentState = InspectState.Idle;
    }
}
