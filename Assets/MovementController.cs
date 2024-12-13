using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : NetworkBehaviour
{
    [SerializeField] private Vector2 moveInput;
    private bool jump;
    [SerializeField] string MoveInputStrng;
    [SerializeField] string fireInputString;
    [SerializeField] float fireInput;       //yea i wonder why its float too
    [SerializeField] float maxSpeed;
    [SerializeField] float maxAcceleration;
    [SerializeField] float drag;
    [SerializeField] bool inputEnabled = false;
    [SerializeField] Transform projectileSpawnPosition;
    private InputAction moveAction;
    private InputAction fireAction;
    private Transform planetTransform;
    [SerializeField] private float gravity = -10;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float distanceToPlanet;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private float shootTimer;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Animator animator;
    public float groundDistance = 1.1f;
    public float groundDistance2;
    [SerializeField] private LayerMask layerMask;
    private Vector3 moveDirection;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject localBullet;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private string animatorFowardTrigger;
    [SerializeField] private string animatorLeftTrigger;
    [SerializeField] private string animatorRightTrigger;
    [SerializeField] private string animatorIdle;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private GameObject[] cubes;

    private void Start()
    {
        if (IsOwner)
        {
            moveAction = InputSystem.actions.FindAction(MoveInputStrng);
            fireAction = InputSystem.actions.FindAction(fireInputString);
            planetTransform = FauxGravitySingleton.Instance.PlanetTransfom;
            inputEnabled = true;
            GameManager.Instance.camera.Follow = gameObject.transform;

        }
    }


    void Update()
    {
        // Move the player
        if (IsOwner)
        {

            //GetInput();
            ////Gravity();        //we dont talk abut gravity here
            ////ApplyDrag();      //we dont talk about drag either
            ////SnapToSphere();
            //HandleRotation();
            ////MoveCharacter();
            //DrawForwardDirection(transform, 2f, Color.green);
            UpdateShootTimer();
            //animateMovement();
            //HandleCameraRotation();

        }






    }
    private void Gravity()
    {
        if (!IsgroundedCheck())
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
    private void HandleCameraRotation()
    {
        GameManager.Instance.camera.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(rotationOffset);

    }
    private void GetInput()
    {
        if (inputEnabled)
        {
            GetFireInput();
            GetMovementDirection();
        }

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
            Vector3 desiredForward = Vector3.ProjectOnPlane(GetComponent<Rigidbody>().linearVelocity, gravityUp).normalized;
            // Create a rotation that looks in the desired forward direction with the correct up vector
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, gravityUp);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    private Vector3 GetMovementDirection()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // Calculate the movement direction in world space
        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        return moveDirection;

    }

    private float GetFireInput()
    {
        fireInput = fireAction.ReadValue<float>();
        if (fireInput > 0)
        {
            if (CanShoot())
            {
                Fire();

            }
        }
        return fireInput;

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
    private bool CanShoot()
    {

        if (shootTimer >= shootCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void UpdateShootTimer()
    {
        shootTimer += Time.deltaTime;
    }
    private void ResetShootTimer()
    {
        shootTimer = 0;
    }
    private void MoveCharacter()
    {
        GetDesiredVelocity();
        //transform.Translate(velocity, Space.World);
    }
    private void GetDesiredVelocity()
    {
        Vector3 acceleration = new Vector3(moveInput.x, 0f, moveInput.y) * maxSpeed;
        velocity += acceleration * Time.deltaTime;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, acceleration.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, acceleration.z, maxSpeedChange);

    }
    void SnapToSphere()
    {

        // Calculate the direction from the planet's center to the character
        Vector3 gravityUp = GetGravityVector();

        // Position the character at the correct distance from the planet's center
        transform.position = planetTransform.position + gravityUp * groundDistance2;
    }
    [ServerRpc]
    public void PlayerHitBehaviourServerRpc()
    {
        ToogleInput(false);
        playerModel.SetActive(false);
        GameObject explosion = Instantiate(explosionVFX, transform);
        explosion.transform.SetParent(null);
        moveInput = Vector2.zero;
        //Implement explosion effect


    }
    public void RequestHITPLayer()
    {
        if (IsHost)
        {
            ToogleInput(false);
            playerModel.SetActive(false);
            GameObject explosion = Instantiate(explosionVFX, transform);
            moveInput = Vector2.zero;
            //Implement explosion effect
        }
        else
        {
            if (IsOwner)
            {
                PlayerHitBehaviourServerRpc();
            }

        }
    }
    private void ToogleInput(bool state)
    {
        inputEnabled = state;
    }
    private void DespawnAllNetworkObjects()
    {
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            // Skip the player objects if you don't want to despawn them
            if (networkObject.IsPlayerObject)
                continue;

            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn(true); // Passing true to destroy the object
            }
        }
    }
    private void ResetSceneBehaviour()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
    }



    void Jump()
    {
        // Implement jump logic
        Debug.Log("Jump");
    }

    void Fire()
    {
        ResetShootTimer();
        if (IsHost)
        {
            NetworkObject bullet = SpawnProjectile(projectileSpawnPosition.position, projectileSpawnPosition.rotation);
            bullet.gameObject.GetComponent<ProjectileBehaviour>().planetTransform = planetTransform;
            bullet.gameObject.GetComponent<ProjectileBehaviour>().bulletDirection = transform.right * (-1);                 //I stared into the abbyss
                                                                                                                            //And abbyss responed...
                                                                                                                            //transform.foward = transform.right * -1
                                                                                                                            //I never looked into the abbyss ever again

        }
        else
        {
            if (IsOwner)
            {
                GameObject bullet = ObjectPool.Instance.GetObject();
                bullet.transform.position = projectileSpawnPosition.position;
                bullet.gameObject.GetComponent<localProjectileBehaviour>().planetTransform = planetTransform;
                bullet.gameObject.GetComponent<localProjectileBehaviour>().bulletDirection = transform.right * (-1);                 //I stared into the abbyss
                float latency = (NetworkManager.Singleton.LocalTime.TimeAsFloat - NetworkManager.Singleton.ServerTime.TimeAsFloat);
                StartCoroutine(DestroyBulletCorutine(latency, bullet));
                Vector3 predictedPosition = bullet.GetComponent<localProjectileBehaviour>().GetFuturePosition(latency);
                RequstNetworkObjectAndFireServerRpc(predictedPosition, projectileSpawnPosition.rotation);

            }
        }
    }
    private IEnumerator DestroyBulletCorutine(float time, GameObject localBulletPrefab)
    {
        yield return new WaitForSeconds(time);
        ObjectPool.Instance.ReturnObject(localBulletPrefab);
    }

    [ServerRpc]
    private void RequstNetworkObjectAndFireServerRpc(Vector3 position, Quaternion rotation)
    {

        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectilePrefab, position, rotation);

        // Spawn the NetworkObject over the network
        networkObject.Spawn();
        networkObject.gameObject.GetComponent<ProjectileBehaviour>().planetTransform = planetTransform;
        networkObject.gameObject.GetComponent<ProjectileBehaviour>().bulletDirection = transform.right * (-1);

    }
    private NetworkObject SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        // Get an instance from the pool
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectilePrefab, position, rotation);

        // Spawn the NetworkObject over the network
        networkObject.Spawn();
        return networkObject;
    }
    private void DrawForwardDirection(Transform transformToDraw, float length = 2f, Color? color = null)
    {
        Color lineColor = color ?? Color.red; // Use provided color or default to red
        Debug.DrawRay(transformToDraw.position, transformToDraw.forward * length, lineColor);
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