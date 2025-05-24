using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private bool autoFindComponents = true;
    [SerializeField] private bool createGround = true;
    
    [Header("Mass Distribution (kg)")]
    [SerializeField] private float faceBoltMass = 0.005f;
    [SerializeField] private float metalWheelMass = 0.035f;
    [SerializeField] private float plasticWheelMass = 0.015f;
    [SerializeField] private float spinTrackMass = 0.008f;
    [SerializeField] private float tipMass = 0.003f;
    
    [Header("Ground Settings")]
    [SerializeField] private Vector3 groundSize = new Vector3(5f, 0.1f, 5f);
    [SerializeField] private Vector3 groundPosition = new Vector3(0f, -1f, 0f);
    
    // Physics Materials
    private PhysicsMaterial metalMaterial;
    private PhysicsMaterial plasticMaterial;
    private PhysicsMaterial rubberMaterial;
    private PhysicsMaterial groundMaterial;
    
    // Component references
    private Rigidbody mainRigidbody;
    private List<Collider> allColliders = new List<Collider>();

    void Start()
    {
        SetupPhysicsMaterials();
        
        if (autoFindComponents)
        {
            FindBeybladeComponents();
        }
        
        SetupBeybladePhysics();
        
        if (createGround)
        {
            CreateGround();
        }
        
        // Apply initial torque after a brief delay to ensure everything is set up
        Invoke(nameof(ApplyInitialTorque), 0.1f);
    }
    
    void SetupPhysicsMaterials()
    {
        // Metal material - low friction, medium bounce
        metalMaterial = new PhysicsMaterial("Metal");
        metalMaterial.dynamicFriction = 0.3f;
        metalMaterial.staticFriction = 0.4f;
        metalMaterial.bounciness = 0.3f;
        metalMaterial.frictionCombine = PhysicsMaterialCombine.Average;
        metalMaterial.bounceCombine = PhysicsMaterialCombine.Average;
        
        // Plastic material - medium friction, low bounce
        plasticMaterial = new PhysicsMaterial("Plastic");
        plasticMaterial.dynamicFriction = 0.5f;
        plasticMaterial.staticFriction = 0.6f;
        plasticMaterial.bounciness = 0.2f;
        plasticMaterial.frictionCombine = PhysicsMaterialCombine.Average;
        plasticMaterial.bounceCombine = PhysicsMaterialCombine.Average;
        
        // Rubber material - high friction, medium bounce (for tip)
        rubberMaterial = new PhysicsMaterial("Rubber");
        rubberMaterial.dynamicFriction = 0.8f;
        rubberMaterial.staticFriction = 0.9f;
        rubberMaterial.bounciness = 0.4f;
        rubberMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;
        rubberMaterial.bounceCombine = PhysicsMaterialCombine.Average;
        
        // Ground material - medium friction
        groundMaterial = new PhysicsMaterial("Ground");
        groundMaterial.dynamicFriction = 0.6f;
        groundMaterial.staticFriction = 0.7f;
        groundMaterial.bounciness = 0.1f;
        groundMaterial.frictionCombine = PhysicsMaterialCombine.Average;
        groundMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
    }
    
    void FindBeybladeComponents()
    {
        // Try to find components by name (case-insensitive)
        Transform[] childTransforms = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in childTransforms)
        {
            string name = child.name.ToLower();
            
            if (name.Contains("facebolt") || name.Contains("face") && name.Contains("bolt"))
                faceBolt = child;
            else if (name.Contains("metalwheel") || name.Contains("metal") && name.Contains("wheel"))
                metalWheel = child;
            else if (name.Contains("plasticwheel") || name.Contains("plastic") && name.Contains("wheel"))
                plasticWheel = child;
            else if (name.Contains("spintrack") || name.Contains("spin") && name.Contains("track"))
                spinTrack = child;
            else if (name.Contains("tip"))
                tip = child;
        }
        
        // Log found components
        Debug.Log($"Found components - FaceBolt: {faceBolt?.name}, MetalWheel: {metalWheel?.name}, " +
                  $"PlasticWheel: {plasticWheel?.name}, SpinTrack: {spinTrack?.name}, Tip: {tip?.name}");
    }
    
    void SetupBeybladePhysics()
    {
        // Add main rigidbody to the parent object
        mainRigidbody = GetComponent<Rigidbody>();
        if (mainRigidbody == null)
        {
            mainRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure main rigidbody
        mainRigidbody.mass = faceBoltMass + metalWheelMass + plasticWheelMass + spinTrackMass + tipMass;
        mainRigidbody.linearDamping = 0.1f;
        mainRigidbody.angularDamping = 0.05f;
        mainRigidbody.useGravity = true;
        mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Setup individual components
        SetupComponent(faceBolt, plasticMaterial, faceBoltMass);
        SetupComponent(metalWheel, metalMaterial, metalWheelMass);
        SetupComponent(plasticWheel, plasticMaterial, plasticWheelMass);
        SetupComponent(spinTrack, plasticMaterial, spinTrackMass);
        SetupComponent(tip, rubberMaterial, tipMass);
        
        // Adjust center of mass (metal wheel is typically the heaviest and should lower center of mass)
        if (metalWheel != null)
        {
            Vector3 centerOfMass = transform.InverseTransformPoint(metalWheel.position);
            centerOfMass.y -= 0.01f; // Lower the center of mass slightly
            mainRigidbody.centerOfMass = centerOfMass;
        }
        
        Debug.Log($"Beyblade physics setup complete. Total mass: {mainRigidbody.mass}kg");
    }
    
    void SetupComponent(Transform component, PhysicsMaterial material, float mass)
    {
        if (component == null) return;
        
        // Get or add mesh collider
        MeshCollider meshCollider = component.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = component.gameObject.AddComponent<MeshCollider>();
        }
        
        // Configure mesh collider
        MeshRenderer meshRenderer = component.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = component.GetComponent<MeshFilter>();
        
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = true; // Required for rigidbodies
            meshCollider.material = material;
        }
        
        allColliders.Add(meshCollider);
        
        // Store mass information for center of mass calculation
        // (We use the main rigidbody instead of individual rigidbodies for better stability)
        
        Debug.Log($"Setup component: {component.name} with material: {material.name}");
    }
    
    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "BeybladeGround";
        ground.transform.position = groundPosition;
        ground.transform.localScale = groundSize;
        
        // Setup ground physics
        Collider groundCollider = ground.GetComponent<Collider>();
        groundCollider.material = groundMaterial;
        
        // Make ground kinematic (immovable)
        Rigidbody groundRb = ground.GetComponent<Rigidbody>();
        if (groundRb == null)
        {
            groundRb = ground.AddComponent<Rigidbody>();
        }
        groundRb.isKinematic = true;
        
        // Optional: Create a better looking material for the ground
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.8f, 0.8f, 0.8f);
        groundMat.SetFloat("_Metallic", 0.1f);
        groundMat.SetFloat("_Glossiness", 0.3f);
        groundRenderer.material = groundMat;
        
        Debug.Log("Ground created for beyblade arena");
    }
    
    void ApplyInitialTorque()
    {
        if (mainRigidbody != null)
        {
            Vector3 torque = torqueAxis.normalized * initialTorque;
            mainRigidbody.AddTorque(torque, ForceMode.Impulse);
            
            Debug.Log($"Applied initial torque: {torque} to beyblade");
        }
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
    
    public void ResetBeyblade()
    {
        if (mainRigidbody != null)
        {
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
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
    
    // Visualize center of mass in scene view
    void OnDrawGizmos()
    {
        if (mainRigidbody != null)
        {
            Gizmos.color = Color.red;
            Vector3 centerOfMass = transform.TransformPoint(mainRigidbody.centerOfMass);
            Gizmos.DrawWireSphere(centerOfMass, 0.01f);
        }
        
        // Draw torque direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, torqueAxis.normalized * 0.1f);
    }
} 