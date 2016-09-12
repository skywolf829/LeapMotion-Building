using UnityEngine;
using System.Collections;

public class collisionDetector : MonoBehaviour {

    void OnCollisionEnter(Collision c)
    {        
        if(c.gameObject.tag == "Finger")
        {
            GameObject hl = GameObject.FindGameObjectWithTag("HandListener");
            hl.BroadcastMessage("lastHandTouchingObject", c.transform.parent.transform.parent.gameObject);
            hl.BroadcastMessage("hittingObject", gameObject);
        }
    }
}
