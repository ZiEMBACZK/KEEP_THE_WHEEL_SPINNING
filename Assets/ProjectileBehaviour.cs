using UnityEngine;
using Unity.Netcode;

public class ProjectileBehaviour : NetworkBehaviour
{
    public Vector3 bulletDirection;
    public float bulletSpeed;
    public Transform planetTransform;
    [SerializeField] private float distanceFromGround;
    [SerializeField] private GameObject bulletPrefab;               //yes we hold referance to bulletPrefab inside bulletPrefab why beacus networkPool need this is this smart no do i care no
    private void Awake()
    {
    }
    private void Start()
    {
        planetTransform = FauxGravitySingleton.Instance.PlanetTransfom;
        GameManager.Instance.onSceneRestart.AddListener(OnSceneReload);
        
    }

    private void Update()
    {
        MoveBullet();
        //HandleRotation();
        SnapToSphere();
    }

    void SnapToSphere()
    {
        Vector3 gravityUp = GetGravityVector();
        transform.position = planetTransform.position + gravityUp * distanceFromGround;
    }

    private void HandleRotation()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, GetGravityVector()) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime); // Adjust the speed of rotation
    }

    private Vector3 GetGravityVector()
    {
        return (transform.position - planetTransform.position).normalized;
    }

    private void MoveBullet()
    {
        Vector3 gravityUp = GetGravityVector();
        Vector3 tangent = Vector3.Cross(gravityUp, bulletDirection.normalized); 

        transform.position += tangent * bulletSpeed * Time.deltaTime;
    }
    private void OnSceneReload()
    {
        Destroy(gameObject);
                                                                                                                 //TOO BAD!
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<MovementController>())
        {
            Debug.Log("Bullet Hit!");
            other.GetComponent<MovementController>().PlayerHitBehaviour();

        }
        
    }
}
