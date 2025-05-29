using UnityEngine;

public class CameraOrbitControls : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // The Beyblade to follow
    
    [Header("Camera Position Settings")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    

    [Header("Camera Behavior")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool useFixedAngle = false;
    [SerializeField] private float fixedAngleY = 45f; // Degrees around Y-axis
    [SerializeField] private bool maintainWorldUp = true;
    
    private Vector3 currentVelocity;
    
    void Start()
    {
        // Auto-find the Beyblade target if not assigned
        if (target == null)
        {
            BeybladeController beyblade = FindObjectOfType<BeybladeController>();
            if (beyblade != null)
            {
                target = beyblade.transform;
                Debug.Log("CameraControls: Auto-assigned target to " + target.name);
            }
            else
            {
                Debug.LogError("CameraControls: No target assigned and no BeybladeController found in scene!");
                enabled = false;
                return;
            }
        }
        
        // Set initial camera position
        if (target != null)
        {
            UpdateCameraPosition(false);
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        UpdateCameraPosition(true);
        UpdateCameraRotation();
    }
    
    private void UpdateCameraPosition(bool useSmoothing)
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        
        if (useSmoothing && smoothFollow)
        {
            // Smooth movement using SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);
        }
        else
        {
            // Instant positioning
            transform.position = desiredPosition;
        }
    }
    
    private Vector3 CalculateDesiredPosition()
    {
        Vector3 targetPosition = target.position;
        
        if (useFixedAngle)
        {
            // Calculate position based on fixed angle around the target
            float angleRad = fixedAngleY * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Sin(angleRad) * followDistance,
                heightOffset,
                Mathf.Cos(angleRad) * followDistance
            );
            return targetPosition + offset + positionOffset;
        }
        else
        {
            // Calculate position relative to current camera direction
            Vector3 directionToCamera = (transform.position - targetPosition).normalized;
            
            // If camera is too close or direction is invalid, use a default direction
            if (directionToCamera.magnitude < 0.1f)
            {
                directionToCamera = Vector3.back + Vector3.up * 0.5f;
                directionToCamera.Normalize();
            }
            
            Vector3 desiredPosition = targetPosition + directionToCamera * followDistance;
            desiredPosition.y = targetPosition.y + heightOffset;
            
            return desiredPosition + positionOffset;
        }
    }
    
    private void UpdateCameraRotation()
    {
        // Calculate direction to look at target
        Vector3 directionToTarget = target.position - transform.position;
        
        if (directionToTarget.magnitude < 0.01f) return;
        
        Quaternion targetRotation;
        
        if (maintainWorldUp)
        {
            // Look at target while maintaining world up direction
            targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        }
        else
        {
            // Look at target with natural rotation
            targetRotation = Quaternion.LookRotation(directionToTarget);
        }
        
        if (smoothRotation)
        {
            // Smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Instant rotation
            transform.rotation = targetRotation;
        }
    }
    
    // Public methods for runtime control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetFollowDistance(float distance)
    {
        followDistance = Mathf.Max(0.1f, distance);
    }
    
    public void SetHeightOffset(float height)
    {
        heightOffset = height;
    }
    
    public void SetFollowSpeed(float speed)
    {
        followSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void ToggleSmoothFollow()
    {
        smoothFollow = !smoothFollow;
    }
    
    public void ToggleSmoothRotation()
    {
        smoothRotation = !smoothRotation;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // Draw connection line from camera to target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        
        // Draw follow distance sphere
        Gizmos.color = Color.cyan * 0.3f;
        Gizmos.DrawWireSphere(target.position, followDistance);
        
        // Draw camera look direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        
        // Draw target position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(target.position, Vector3.one * 0.1f);
    }
} 