using UnityEngine;
using UnityEngine.InputSystem;

public class BeybladeController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float rotationForce = 5f;
    
    [Header("Input Settings")]
    [SerializeField] private Key forwardKey = Key.W;
    [SerializeField] private Key backwardKey = Key.S;
    [SerializeField] private Key leftKey = Key.A;
    [SerializeField] private Key rightKey = Key.D;
    
    [Header("Force Visualization")]
    [SerializeField] private bool showForceVisualization = true;
    [SerializeField] private Color forceColor = Color.green;
    [SerializeField] private Color inputDirectionColor = Color.cyan;
    [SerializeField] private float forceArrowScale = 0.1f;
    [SerializeField] private bool showForceLabels = true;
    [SerializeField] private bool showInputDirection = true;
    
    // Component references
    private BeybladePhysicsSetup physicsSetup;
    private Rigidbody rb;
    
    // Force tracking for visualization
    private Vector3 currentInputDirection;
    private Vector3 currentAppliedForce;
    private bool isApplyingForce;
    
    void Start()
    {
        // Get references to required components
        physicsSetup = GetComponent<BeybladePhysicsSetup>();
        rb = GetComponent<Rigidbody>();
        
        if (physicsSetup == null)
        {
            Debug.LogError("BeybladeController requires BeybladePhysicsSetup component!");
            enabled = false;
            return;
        }
        
        if (rb == null)
        {
            Debug.LogError("BeybladeController requires Rigidbody component!");
            enabled = false;
            return;
        }
        
        Debug.Log("BeybladeController initialized. Use WASD to control the Beyblade.");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Get input directions using new Input System
        Vector3 inputDirection = Vector3.zero;
        
        if (Keyboard.current[forwardKey].isPressed)
        {
            inputDirection += Vector3.forward;
        }
        if (Keyboard.current[backwardKey].isPressed)
        {
            inputDirection += Vector3.back;
        }
        if (Keyboard.current[leftKey].isPressed)
        {
            inputDirection += Vector3.left;
        }
        if (Keyboard.current[rightKey].isPressed)
        {
            inputDirection += Vector3.right;
        }
        
        // Store input direction for visualization
        currentInputDirection = inputDirection;
        
        // Apply movement if there's input
        if (inputDirection != Vector3.zero)
        {
            // Normalize to prevent faster diagonal movement
            inputDirection.Normalize();
            
            // Apply movement force
            ApplyMovementForce(inputDirection);
            isApplyingForce = true;
        }
        else
        {
            // No input, reset force tracking
            currentAppliedForce = Vector3.zero;
            isApplyingForce = false;
        }
    }
    
    private void ApplyMovementForce(Vector3 direction)
    {
        // Use input direction directly as world space direction
        // W = +Z, S = -Z, A = -X, D = +X (world coordinates)
        Vector3 worldDirection = direction;
        
        // Check current velocity to prevent exceeding max speed
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        if (horizontalVelocity.magnitude < maxMoveSpeed)
        {
            // Calculate the actual force being applied
            Vector3 appliedForce = worldDirection * moveForce;
            currentAppliedForce = appliedForce;
            
            // Apply force through the physics setup
            physicsSetup.ApplyMovementForce(appliedForce);
        }
        else
        {
            // No force applied due to speed limit
            currentAppliedForce = Vector3.zero;
        }
    }
    
    // Public methods for external control
    public void SetMoveForce(float newForce)
    {
        moveForce = Mathf.Max(0, newForce);
    }
    
    public void SetMaxMoveSpeed(float newSpeed)
    {
        maxMoveSpeed = Mathf.Max(0, newSpeed);
    }
    
    public float GetCurrentMoveSpeed()
    {
        if (rb != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            return horizontalVelocity.magnitude;
        }
        return 0f;
    }
    
    // Debug visualization
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Display current speeds in the corner
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Move Speed: {GetCurrentMoveSpeed():F1} / {maxMoveSpeed:F1} m/s");
        GUILayout.Label($"Spin Speed: {physicsSetup.GetCurrentSpinSpeed():F1} rad/s");
        GUILayout.Label("");
        GUILayout.Label("Controls:");
        GUILayout.Label("W/A/S/D - Move");
        
        // Add force information
        if (isApplyingForce)
        {
            GUILayout.Label("");
            GUILayout.Label($"Applied Force: {currentAppliedForce.magnitude:F1} N");
            GUILayout.Label($"Force Direction: {currentAppliedForce.normalized}");
        }
        
        GUILayout.EndArea();
    }
    
    // Force visualization in Scene view
    void OnDrawGizmos()
    {
        if (!showForceVisualization || !Application.isPlaying) return;
        
        DrawForceVisualization();
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showForceVisualization) return;
        
        DrawForceVisualization();
        
        if (showForceLabels)
        {
            DrawForceLabels();
        }
    }
    
    private void DrawForceVisualization()
    {
        Vector3 centerPos = transform.position;
        
        // Draw input direction (local space)
        if (showInputDirection && currentInputDirection != Vector3.zero)
        {
            Gizmos.color = inputDirectionColor;
            Vector3 inputDir = currentInputDirection.normalized * 0.5f;
            Vector3 localInputEnd = centerPos + inputDir;
            
            // Draw input direction arrow
            Gizmos.DrawRay(centerPos, inputDir);
            DrawArrowHead(localInputEnd, inputDir.normalized, 0.1f, inputDirectionColor);
        }
        
        // Draw applied force (world space)
        if (isApplyingForce && currentAppliedForce != Vector3.zero)
        {
            Gizmos.color = forceColor;
            Vector3 forceDir = currentAppliedForce.normalized * (currentAppliedForce.magnitude * forceArrowScale);
            Vector3 forceEnd = centerPos + forceDir;
            
            // Draw force vector
            Gizmos.DrawRay(centerPos, forceDir);
            DrawArrowHead(forceEnd, forceDir.normalized, 0.15f, forceColor);
            
            // Draw force magnitude indicator (thicker line for stronger forces)
            float thickness = Mathf.Clamp(currentAppliedForce.magnitude * 0.01f, 0.01f, 0.1f);
            Gizmos.color = forceColor * 0.7f;
            Gizmos.DrawWireSphere(centerPos, thickness);
        }
    }
    
    private void DrawArrowHead(Vector3 position, Vector3 direction, float size, Color color)
    {
        Gizmos.color = color;
        
        // Calculate perpendicular vectors for arrowhead
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right == Vector3.zero) right = Vector3.Cross(direction, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, direction).normalized;
        
        // Draw arrowhead lines
        Vector3 arrowBase = position - direction * size;
        Vector3 arrowRight = arrowBase + right * size * 0.5f;
        Vector3 arrowLeft = arrowBase - right * size * 0.5f;
        Vector3 arrowUp = arrowBase + up * size * 0.5f;
        Vector3 arrowDown = arrowBase - up * size * 0.5f;
        
        Gizmos.DrawLine(position, arrowRight);
        Gizmos.DrawLine(position, arrowLeft);
        Gizmos.DrawLine(position, arrowUp);
        Gizmos.DrawLine(position, arrowDown);
    }
    
    private void DrawForceLabels()
    {
#if UNITY_EDITOR
        Vector3 labelPos = transform.position + Vector3.up * 0.8f;
        
        if (isApplyingForce && currentAppliedForce != Vector3.zero)
        {
            string forceInfo = $"Force: {currentAppliedForce.magnitude:F1} N";
            UnityEditor.Handles.Label(labelPos, forceInfo);
            
            string directionInfo = $"Dir: ({currentAppliedForce.normalized.x:F2}, {currentAppliedForce.normalized.y:F2}, {currentAppliedForce.normalized.z:F2})";
            UnityEditor.Handles.Label(labelPos + Vector3.up * 0.2f, directionInfo);
        }
        
        if (showInputDirection && currentInputDirection != Vector3.zero)
        {
            string inputInfo = $"Input: ({currentInputDirection.x:F1}, {currentInputDirection.y:F1}, {currentInputDirection.z:F1})";
            UnityEditor.Handles.Label(labelPos + Vector3.up * 0.4f, inputInfo);
        }
#endif
    }
} 