using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class Controller : MonoBehaviour
{
    [Header("Camera Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Movement Settings")]
    [SerializeField] private LayerMask walkableLayer = ~0;
    [SerializeField] private float maxClickDistance = 100f;

    [Header("Click Effect Settings")]
    [SerializeField] private GameObject clickEffectPrefab;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private float effectLifetime = 2f;

    private NavMeshAgent agent;
    private float verticalRotation = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        HandleClickToMove();
        HandleCameraLook();
    }

    private void HandleClickToMove()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = playerCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, walkableLayer))
            {
                agent.SetDestination(hit.point);
                SpawnClickEffect(hit.point, hit.normal);
            }
        }
    }

    private void SpawnClickEffect(Vector3 position, Vector3 surfaceNormal)
    {
        if (clickEffectPrefab == null) return;

        Vector3 spawnPosition = position + (surfaceNormal * surfaceOffset);

        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.up);

        GameObject spawnedEffect = Instantiate(clickEffectPrefab, spawnPosition, spawnRotation);

        Destroy(spawnedEffect, effectLifetime);
    }

    private void HandleCameraLook()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}
