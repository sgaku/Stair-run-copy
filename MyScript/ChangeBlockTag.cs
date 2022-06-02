using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBlockTag : MonoBehaviour
{ 
    
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Untagged"; 
        }
    }
}
