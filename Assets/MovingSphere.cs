using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingSphere : NetworkBehaviour
{

    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    Rigidbody body;

    Vector3 velocity, desiredVelocity;

    Vector3 upAxis, rightAxis, forwardAxis;

    bool desiredJump;

    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepContactCount;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;
    Vector3 playerInput;
    Animator animator;
    [SerializeField] Transform projectileSpawnPosition;
    [SerializeField] private float shootTimer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform planetTransform;
    [SerializeField] private float shootCooldown;
    private InputAction fireAction;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private bool noInput;
    [SerializeField] public bool isDead;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        if (!IsOwner)
            return;
    }
    private void Start()
    {
        if (!IsOwner)
            return;
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        OnValidate();
        animator = GetComponentInChildren<Animator>();
        Camera cam = GameManager.Instance.playerCamera;
        playerInputSpace = cam.transform;
        cam.GetComponent<OrbitCamera>().focus = gameObject.transform;
        cam.GetComponent<OrbitCamera>().enabled = true;
        StartCoroutine(GameManager.Instance.AnimateCamera());
        planetTransform = GameManager.Instance.planetTransform;
        fireAction = InputSystem.actions.FindAction("Fire");

    }

    void Update()
    {
        if (!IsOwner)
            return;
        UpdateShootTimer();
        AnimateMovement();
        if(!noInput)
        {
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            GetFireInput();

        }
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
        desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        //desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;
        if (noInput)
            return;
          
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        velocity += gravity * Time.deltaTime;

        body.linearVelocity = velocity;
        ClearState();
    }
    private void AnimateMovement()
    {
        if (animator == null)
            return;
        if (playerInput.y > 0)
        {
            animator.SetFloat("L_speed", 1);
            animator.SetFloat("R_speed", 1);
        }
        else
        {
            animator.SetFloat("L_speed", -1);
            animator.SetFloat("R_speed", -1);
        }
        if (playerInput.y < 0.05f && playerInput.x > 0)
        {
            animator.SetFloat("L_speed", 1);
            animator.SetFloat("R_speed", -1);
        }
        if (playerInput.y < 0.05f && playerInput.x < 0)
        {
            animator.SetFloat("L_speed", -1);
            animator.SetFloat("R_speed", 1);
        }
        if (playerInput == Vector3.zero)
        {
            animator.SetFloat("L_speed", 0);
            animator.SetFloat("R_speed", 0);
        }
    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void UpdateState()
    {

        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.linearVelocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
        if (velocity.magnitude > 0.1f)
        {
            //Vector3 desiredForward = Vector3.ProjectOnPlane(GetComponent<Rigidbody>().linearVelocity, upAxis).normalized;
            Vector3 desiredForward = ProjectDirectionOnPlane(velocity, upAxis);
            // Create a rotation that looks in the desired forward direction with the correct up vector
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, upAxis);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        }
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(
            body.position, -upAxis, out RaycastHit hit,
            probeDistance, probeMask
        ))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
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
    private void ResetShootTimer()
    {
        shootTimer = 0;
    }
    private IEnumerator DestroyBulletCorutine(float time, GameObject localBulletPrefab)
    {
        yield return new WaitForSeconds(time);
        ObjectPool.Instance.ReturnObject(localBulletPrefab);
    }
    private float GetFireInput()
    {
        float fireInput = 0;
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
    public void RequestHITPLayer()
    {
        if (IsHost)
        {
            playerModel.SetActive(false);
            GameObject explosion = Instantiate(explosionVFX, transform);
            noInput = true;
            isDead = true;
            GameManager.Instance.ChangeScoreServerRpc();
            PlayerDeathBeahaviourClientRpc();
                StartCoroutine(RespawnCourutineClient());

            //Implement explosion effect
        }
        else
        {
            if (IsOwner)
            {
                playerModel.SetActive(false);
                GameObject explosion = Instantiate(explosionVFX, transform);
                noInput = true;
                isDead = true;
                body.isKinematic = true;
                GameManager.Instance.ChangeScoreServerRpc();
                StartCoroutine(RespawnCourutineHost());
                
            }

        }
    }
    private IEnumerator RespawnCourutineClient()
    {
        yield return new WaitForSeconds(3);
        playerModel.SetActive(true);
        noInput = false;
        isDead = false;
        RespawnPlayerClientRpc();
        
    }
    private IEnumerator RespawnCourutineHost()
    {
        yield return new WaitForSeconds(3);
        playerModel.SetActive(true);
        noInput = false;
        isDead = false;
        RespawnPlayerServerRpc();
        
    }
    [ServerRpc(RequireOwnership =false)]
    public void PlayerDeathBeahaviourServerRpc(ServerRpcParams rpcparams = default)
    {
        playerModel.SetActive(false);
        GameObject explosion = Instantiate(explosionVFX, transform);
    }
    [ClientRpc]
    public void PlayerDeathBeahaviourClientRpc()
    {
        playerModel.SetActive(false);
        GameObject explosion = Instantiate(explosionVFX, transform);
    }
    [ServerRpc(RequireOwnership = default)]
    public void RespawnPlayerServerRpc()
    {
        playerModel.SetActive(true);
        noInput = false;
        isDead = false;
        transform.position = GameManager.Instance.spawnPoints[Random.Range(0, 3)].position;

    }
    [ClientRpc(RequireOwnership = false)]
    public void RespawnPlayerClientRpc()
    {
        playerModel.SetActive(true);
        noInput = false;
        isDead = false;
        body.isKinematic = false;
        transform.position = GameManager.Instance.spawnPoints[Random.Range(0, 3)].position;

    }
    private void FreezPosition()
    {
    }

}
