using UnityEngine;

public class FauxGravityAttractor : MonoBehaviour
{
    public float gravity = -10f;
    public void Attract(Transform bodyTrans, CharacterController cc)
    {
        Vector3 gravityUp = (bodyTrans.position - transform.position).normalized; // Vektor do srodka planety
        Vector3 bodyUp = bodyTrans.up; //vektor w gore playera

        if(cc.isGrounded)
        {
            cc.Move(gravityUp * Time.deltaTime * 2f);
        }
        else
        {
            cc.Move(gravityUp * Time.deltaTime * gravity);
        }
        

       // Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * bodyTrans.rotation; //
       // bodyTrans.rotation = Quaternion.Slerp(bodyTrans.rotation, targetRotation, 50 * Time.deltaTime); //rotacja playera wzgledem planety


    }
}
