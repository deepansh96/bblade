using UnityEngine;

public class BeybladePhysicsSetup : MonoBehaviour
{
    [Header("Beyblade Components")]
    [SerializeField] private Transform faceBolt;
    [SerializeField] private Transform metalWheel;
    [SerializeField] private Transform plasticWheel;
    [SerializeField] private Transform spinTrack;
    [SerializeField] private Transform tip;
    
    [Header("Physics Settings")]
    [SerializeField] private float initialTorque = 100f;
    [SerializeField] private Vector3 torqueAxis = Vector3.up;
    [SerializeField] private float maxAngularVelocity = 200f;
    
    [Header("Center of Mass Settings")]
    [SerializeField] private bool enableCustomCenterOfMass = false;
    [SerializeField] private Transform referenceTransform;
    
    [Header("Mass Distribution (kg) - Reference Only")]
    [SerializeField] private float faceBoltMass = 0.005f;
    [SerializeField] private float metalWheelMass = 0.035f;
    [SerializeField] private float plasticWheelMass = 0.015f;
    [SerializeField] private float spinTrackMass = 0.008f;
    [SerializeField] private float tipMass = 0.003f;
    
    [Header("Scene View Visualization")]
    [SerializeField] private bool showCenterOfMass = true;
    [SerializeField] private bool showCenterOfMassLabel = true;
    [SerializeField] private Color centerOfMassColor = Color.red;
    [SerializeField] private float centerOfMassSize = 0.02f;
    [SerializeField] private bool showTorqueDirection = true;
    [SerializeField] private Color torqueDirectionColor = Color.yellow;
    [SerializeField] private float torqueArrowLength = 0.1f;
    
    [Header("Time Control")]
    [SerializeField] private bool slowMode = false;
    [SerializeField] private float slowModeTimeScale = 0.1f;
    [SerializeField] private float normalTimeScale = 1.0f;
    
    [Header("Inertia Tensor Control")]
    [SerializeField] private float automaticTensorThreshold = 50f;
    [SerializeField] private Vector3 customInertiaTensor = new Vector3(1f, 2f, 1f);
    [SerializeField] private Vector3 customInertiaTensorRotation = Vector3.zero;
    [SerializeField] private bool showTensorInfo = true;
    
    // Component references
    private Rigidbody mainRigidbody;
    
    // Tensor control state
    private bool hasAutomaticTensorActivated = false;
    private bool canStartMonitoring = false;

    void Awake()
    {
        // Get the rigidbody component early
        mainRigidbody = GetComponent<Rigidbody>();
        
        if (mainRigidbody != null)
        {
            // Set up custom inertia tensor
            SetupCustomInertiaTensor();
        }
        
        // Time scale is now controlled by the slowMode toggle in Update()
    }

    void Start()
    {
        // Get the manually set up rigidbody
        mainRigidbody = GetComponent<Rigidbody>();
        
        if (mainRigidbody == null)
        {
            return;
        }
        
        // Configure physics settings
        ConfigurePhysics();
        
        // Apply initial torque after a brief delay to ensure everything is set up
        Invoke(nameof(ApplyInitialTorque), 0.01f);
    }
    
    void Update()
    {
        // Update time scale based on slow mode toggle
        // This allows for real-time changes from the inspector
        Time.timeScale = slowMode ? slowModeTimeScale : normalTimeScale;
        
        // Monitor angular velocity for automatic tensor switching
        // Only start monitoring after initial torque has been applied
        if (canStartMonitoring && mainRigidbody != null && !hasAutomaticTensorActivated)
        {
            float currentAngularVelocity = mainRigidbody.angularVelocity.magnitude;
            
            if (currentAngularVelocity < automaticTensorThreshold)
            {
                // Switch to automatic inertia tensor
                mainRigidbody.ResetInertiaTensor();
                Debug.Log("Reset inertia tensor" + mainRigidbody.inertiaTensor);
                // mainRigidbody.automaticInertiaTensor = true;
                hasAutomaticTensorActivated = true;
            }
        }
    }
    
    void ConfigurePhysics()
    {
        if (mainRigidbody == null) 
        {
            return;
        }
        
        // Set the maximum angular velocity
        mainRigidbody.maxAngularVelocity = maxAngularVelocity;
        
        // Configure center of mass
        ConfigureCenterOfMass();
        
        // Ensure the rigidbody is awake
        mainRigidbody.WakeUp();
    }
    
    void ConfigureCenterOfMass()
    {
        if (mainRigidbody == null) return;
        
        if (enableCustomCenterOfMass)
        {
            if (referenceTransform != null)
            {
                // Disable automatic center of mass calculation
                mainRigidbody.automaticCenterOfMass = false;
                
                // Convert the reference transform's local position to local space relative to this rigidbody
                Vector3 localCenterOfMass = transform.InverseTransformPoint(referenceTransform.position);
                
                // Set the custom center of mass
                mainRigidbody.centerOfMass = localCenterOfMass;
            }
            else
            {
                // Fall back to automatic center of mass
                mainRigidbody.automaticCenterOfMass = true;
            }
        }
        else
        {
            // Use automatic center of mass calculation
            mainRigidbody.automaticCenterOfMass = true;
        }
    }
    
    void ApplyInitialTorque()
    {
        if (mainRigidbody != null)
        {
            Vector3 torque = torqueAxis.normalized * initialTorque;
            mainRigidbody.AddTorque(torque, ForceMode.Impulse);
            
            // Check angular velocity again after a physics update
            Invoke(nameof(CheckAngularVelocityAfterPhysics), 0.02f);
            
            // Enable monitoring AFTER physics has had time to process the torque
            // This prevents premature switching to automatic tensor
            Invoke(nameof(EnableMonitoring), 5f);
        }
    }
    
    void EnableMonitoring()
    {
        canStartMonitoring = true;
    }
    
    void CheckAngularVelocityAfterPhysics()
    {
        if (mainRigidbody != null)
        {
            // If angular velocity is still zero, try alternative methods
            if (mainRigidbody.angularVelocity.magnitude < 0.1f)
            {
                // Try directly setting angular velocity
                Vector3 targetAngularVelocity = torqueAxis.normalized * 50f; // Start with 50 rad/s
                mainRigidbody.angularVelocity = targetAngularVelocity;
                
                // Check again after another frame
                Invoke(nameof(CheckDirectVelocitySet), 0.02f);
            }
        }
    }
    
    void CheckDirectVelocitySet()
    {
        // Method kept for consistency with Invoke calls
    }
    
    // Public methods for runtime control
    public void AddTorque(float torqueAmount)
    {
        if (mainRigidbody != null)
        {
            Vector3 torque = torqueAxis.normalized * torqueAmount;
            mainRigidbody.AddTorque(torque, ForceMode.Impulse);
        }
    }
    
    public void ApplyMovementForce(Vector3 force)
    {
        if (mainRigidbody != null)
        {
            mainRigidbody.AddForce(force, ForceMode.Force);
        }
    }
    
    public void ResetBeyblade()
    {
        if (mainRigidbody != null)
        {
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            
            // Reconfigure physics after reset
            ConfigurePhysics();
            
            // Reset tensor control state
            ResetTensorControl();
        }
    }
    
    public float GetCurrentSpinSpeed()
    {
        if (mainRigidbody != null)
        {
            return mainRigidbody.angularVelocity.magnitude;
        }
        return 0f;
    }
    
    public void UpdateCenterOfMass()
    {
        ConfigureCenterOfMass();
    }
    
    public void SetCustomCenterOfMass(bool enable, Transform reference = null)
    {
        enableCustomCenterOfMass = enable;
        if (reference != null)
        {
            referenceTransform = reference;
        }
        ConfigureCenterOfMass();
    }
    
    private void SetupCustomInertiaTensor()
    {
        if (mainRigidbody != null)
        {
            // Disable automatic inertia tensor calculation
            mainRigidbody.automaticInertiaTensor = false;
            
            // Set custom inertia tensor values
            mainRigidbody.inertiaTensor = customInertiaTensor;
            mainRigidbody.inertiaTensorRotation = Quaternion.Euler(customInertiaTensorRotation);
            
            // Reset the activation flag
            hasAutomaticTensorActivated = false;
        }
    }
    
    public void ResetTensorControl()
    {
        hasAutomaticTensorActivated = false;
        canStartMonitoring = false;
        SetupCustomInertiaTensor();
    }
    
    // Visualize center of mass in scene view
    void OnDrawGizmos()
    {
        // Draw center of mass visualization
        if (showCenterOfMass)
        {
            DrawCenterOfMass();
        }
        
        // Draw torque direction
        if (showTorqueDirection)
        {
            DrawTorqueDirection();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Enhanced visualization when object is selected
        if (showCenterOfMass)
        {
            DrawCenterOfMass();
            DrawCenterOfMassInfo();
        }
        
        if (showTorqueDirection)
        {
            DrawTorqueDirection();
        }
    }
    
    private void DrawCenterOfMass()
    {
        Gizmos.color = centerOfMassColor;
        
        // Get rigidbody reference directly for edit-time visualization
        Rigidbody rb = GetComponent<Rigidbody>();
        
        Vector3 centerOfMass;
        if (rb != null)
        {
            // Use actual rigidbody center of mass
            centerOfMass = transform.TransformPoint(rb.centerOfMass);
        }
        else
        {
            // Fallback to object center if no rigidbody
            centerOfMass = transform.position;
        }
        
        // Draw main sphere
        Gizmos.DrawWireSphere(centerOfMass, centerOfMassSize);
        Gizmos.DrawSphere(centerOfMass, centerOfMassSize * 0.5f);
        
        // Draw crosshair for better visibility
        float crossSize = centerOfMassSize * 2f;
        Gizmos.DrawLine(centerOfMass - Vector3.right * crossSize, centerOfMass + Vector3.right * crossSize);
        Gizmos.DrawLine(centerOfMass - Vector3.up * crossSize, centerOfMass + Vector3.up * crossSize);
        Gizmos.DrawLine(centerOfMass - Vector3.forward * crossSize, centerOfMass + Vector3.forward * crossSize);
        
        // Draw label if enabled
        if (showCenterOfMassLabel)
        {
#if UNITY_EDITOR
            string label = enableCustomCenterOfMass ? "Center of Mass (Custom)" : "Center of Mass (Auto)";
            UnityEditor.Handles.Label(centerOfMass + Vector3.up * 0.05f, label);
#endif
        }
    }
    
    private void DrawCenterOfMassInfo()
    {
        // Get rigidbody reference directly for edit-time visualization
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;
        
        Vector3 centerOfMass = transform.TransformPoint(rb.centerOfMass);
        
        // Draw line from object center to center of mass
        Gizmos.color = centerOfMassColor * 0.7f;
        Gizmos.DrawLine(transform.position, centerOfMass);
        
        // Calculate and display offset
        Vector3 offset = rb.centerOfMass;
        
#if UNITY_EDITOR
        // Use face bolt position as reference, or fallback to transform position
        Vector3 labelBasePos = faceBolt != null ? faceBolt.position : transform.position;
        
        string info = $"CoM Offset: ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})";
        UnityEditor.Handles.Label(labelBasePos + Vector3.up * 0.80f, info);
        
        // Display total mass if available
        string massInfo = $"Total Mass: {rb.mass:F3} kg";
        UnityEditor.Handles.Label(labelBasePos + Vector3.up * 1.00f, massInfo);
        
        // Display center of mass mode
        string comModeInfo = enableCustomCenterOfMass ? "CoM Mode: Custom" : "CoM Mode: Automatic";
        UnityEditor.Handles.Label(labelBasePos + Vector3.up * 1.20f, comModeInfo);
        
        // Display reference transform info if using custom center of mass
        if (enableCustomCenterOfMass && referenceTransform != null)
        {
            string refInfo = $"Reference: {referenceTransform.name}";
            UnityEditor.Handles.Label(labelBasePos + Vector3.up * 1.40f, refInfo);
        }
        
        // Display adjusted physics values during play mode
        if (Application.isPlaying)
        {
            float yOffset = enableCustomCenterOfMass && referenceTransform != null ? 1.60f : 1.40f;
            string physicsInfo = $"Max Angular Vel: {rb.maxAngularVelocity:F0} rad/s";
            UnityEditor.Handles.Label(labelBasePos + Vector3.up * yOffset, physicsInfo);
            
            // Display tensor information if enabled
            if (showTensorInfo)
            {
                yOffset += 0.20f;
                string tensorMode = hasAutomaticTensorActivated ? "Automatic" : "Custom";
                string tensorInfo = $"Inertia Tensor: {tensorMode}";
                UnityEditor.Handles.Label(labelBasePos + Vector3.up * yOffset, tensorInfo);
                
                if (!hasAutomaticTensorActivated)
                {
                    yOffset += 0.20f;
                    string tensorValues = $"Tensor: ({rb.inertiaTensor.x:F1}, {rb.inertiaTensor.y:F1}, {rb.inertiaTensor.z:F1})";
                    UnityEditor.Handles.Label(labelBasePos + Vector3.up * yOffset, tensorValues);
                }
                
                yOffset += 0.20f;
                float currentAngVel = mainRigidbody != null ? mainRigidbody.angularVelocity.magnitude : 0f;
                string angVelInfo = $"Current Angular Vel: {currentAngVel:F1} rad/s";
                UnityEditor.Handles.Label(labelBasePos + Vector3.up * yOffset, angVelInfo);
                
                if (!hasAutomaticTensorActivated)
                {
                    yOffset += 0.20f;
                    string thresholdInfo = $"Auto Threshold: {automaticTensorThreshold:F0} rad/s";
                    UnityEditor.Handles.Label(labelBasePos + Vector3.up * yOffset, thresholdInfo);
                }
            }
        }
#endif
    }
    
    private void DrawTorqueDirection()
    {
        Gizmos.color = torqueDirectionColor;
        Vector3 torqueDir = torqueAxis.normalized * torqueArrowLength;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + torqueDir;
        
        // Draw main arrow
        Gizmos.DrawRay(startPos, torqueDir);
        
        // Draw arrowhead
        Vector3 arrowHead1 = endPos - torqueDir.normalized * 0.02f + Vector3.Cross(torqueDir, Vector3.forward).normalized * 0.01f;
        Vector3 arrowHead2 = endPos - torqueDir.normalized * 0.02f + Vector3.Cross(torqueDir, Vector3.right).normalized * 0.01f;
        
        Gizmos.DrawLine(endPos, arrowHead1);
        Gizmos.DrawLine(endPos, arrowHead2);
        
#if UNITY_EDITOR
        if (showCenterOfMassLabel)
        {
            // Use face bolt position as reference, or fallback to transform position
            Vector3 labelBasePos = faceBolt != null ? faceBolt.position : transform.position;
            
            UnityEditor.Handles.Label(labelBasePos + Vector3.up * 1.60f, $"Torque ({initialTorque})");
            
            // Show current angular velocity during play mode
            if (Application.isPlaying && mainRigidbody != null)
            {
                float currentSpeed = mainRigidbody.angularVelocity.magnitude;
                string speedInfo = $"Angular Vel: {currentSpeed:F1} rad/s";
                UnityEditor.Handles.Label(labelBasePos + Vector3.up * 1.80f, speedInfo);
                
                string maxSpeedInfo = $"Max: {mainRigidbody.maxAngularVelocity:F0} rad/s";
                UnityEditor.Handles.Label(labelBasePos + Vector3.up * 2.00f, maxSpeedInfo);
            }
        }
#endif
    }
} 