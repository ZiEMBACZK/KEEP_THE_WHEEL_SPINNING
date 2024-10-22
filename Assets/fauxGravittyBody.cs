using UnityEngine;

public class fauxGravittyBody : MonoBehaviour
{
    public FauxGravityAttractor attractor;
    private Transform trans;
    private CharacterController cc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        trans = GetComponent<Transform>();
        cc = GetComponent<CharacterController>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {

    }
}
