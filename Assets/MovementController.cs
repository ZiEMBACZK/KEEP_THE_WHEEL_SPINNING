using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MovementController : NetworkBehaviour
{
    [SerializeField] private Vector2 moveInput;
    private bool jump;
    [SerializeField] string MoveInputStrng;
    [SerializeField] string fireInputString;
    [SerializeField] float speed;
    [SerializeField] float drag;
    private InputAction moveAction;
    private InputAction fireAction;
    private Transform planetTransform;
    [SerializeField] private float gravity = -10;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float distanceToPlanet;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private float rotationSpeed;
    public float groundDistance = 1.1f;  // Distance to the ground to be considered grounded
    public float groundDistance2;
    [SerializeField] private LayerMask layerMask;
    private Vector3 moveDirection;
    [SerializeField] private Camera playerCamera;


    private void Start()
    {
        if (IsOwner)
        {
            moveAction = InputSystem.actions.FindAction(MoveInputStrng);
            fireAction = InputSystem.actions.FindAction(fireInputString);
            planetTransform = FauxGravitySingleton.Instance.PlanetTransfom;
            playerCamera.gameObject.SetActive(true);

        }
    }


    void Update()
    {
        // Move the player
        if (IsOwner) {

            Gravity();
            //ApplyDrag();
            SnapToSphere();
            GetMovementDirection();
            HandleRotation();
            MoveCharacter();
        
        
        
        }






    }
    private void Gravity()
    {
        if(!IsgroundedCheck())
        {
            //velocity += (GetGravityVector() * gravity) * Time.deltaTime;
            //GetComponent<Rigidbody>().AddForce(GetGravityVector() * gravity * Time.deltaTime);
        }
        else
        {
           // velocity = Vector3.zero;
        }
        Debug.Log(IsgroundedCheck());

    }
    private void ApplyDrag()
    {
        velocity.x *= (1 - drag * Time.deltaTime);
        velocity.y *= (1 - drag * Time.deltaTime);
    }
    private void HandleRotation()
    {
        //Quaternion targetRotation = Quaternion.FromToRotation(transform.up, GetGravityVector()) * transform.rotation;
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10000f * Time.deltaTime);

        // Get the gravity up vector
        Vector3 gravityUp = GetGravityVector();

        // If there's movement input, rotate to face the movement direction
        if (moveInput.magnitude > 0.1f)
        {
            // Project the movement direction onto the plane perpendicular to gravity
            Vector3 desiredForward = Vector3.ProjectOnPlane(moveDirection, gravityUp).normalized;

            // Create a rotation that looks in the desired forward direction with the correct up vector
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, gravityUp);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // When there's no movement input, adjust only the up vector

            // Get the current forward direction
            Vector3 currentForward = transform.forward;

            // Project the current forward onto the plane perpendicular to gravity
            Vector3 projectedForward = Vector3.ProjectOnPlane(currentForward, gravityUp).normalized;

            // Handle the case when the projected forward vector is too small
            if (projectedForward.sqrMagnitude < 0.001f)
            {
                // Use a default forward vector perpendicular to gravityUp
                projectedForward = Vector3.Cross(transform.right, gravityUp).normalized;
            }

            // Create a rotation with the current forward and adjusted up vector
            Quaternion currentRotation = Quaternion.LookRotation(projectedForward, gravityUp);

            // Apply the rotation
            transform.rotation = currentRotation;
        }
    }
    private Vector3 GetMovementDirection()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // Calculate the movement direction in world space
        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        return moveDirection;

    }
    private Vector3 GetGravityVector()
    {
        Vector3 directionToPlanet = (transform.position - planetTransform.position).normalized; // Vektor do srodka planety
        return directionToPlanet;

    }
    private bool IsgroundedCheck()
    {
        bool isGrounded = Physics.Raycast(transform.position, -GetGravityVector(), groundDistance, layerMask);
        Debug.DrawRay(transform.position, -GetGravityVector() * groundDistance, Color.red);
        return isGrounded;

    }
    private void MoveCharacter()
    {
        Vector2 movementVector = GetMovementDirection();

        transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);
    }
    void SnapToSphere()
    {

        // Calculate the direction from the planet's center to the character
        Vector3 gravityUp = GetGravityVector();

        // Position the character at the correct distance from the planet's center
        transform.position = planetTransform.position + gravityUp * groundDistance2;
    }



    void Jump()
    {
        // Implement jump logic
        Debug.Log("Jump");
    }

    void Fire()
    {
        // Implement fire logic
        Debug.Log("Fire");
    }
    void OnDrawGizmos()
    {
        if (planetTransform != null)
        {
            // Draw gravity vector
            Vector3 gravityVector = GetGravityVector();

            Gizmos.color = Color.blue; // Set the color for the vector
            Gizmos.DrawLine(transform.position, transform.position + gravityVector); // Draw the gravity line from the character to the planet
        }
    }
}