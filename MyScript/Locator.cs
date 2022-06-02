using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locator : MonoBehaviour
{

    public PlayerController playerController;
    public ObjectPooling objectPooling;

    public static Locator i;
    void Awake()
    {
        i = this;
    }


}
