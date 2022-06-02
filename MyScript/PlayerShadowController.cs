using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの影の大きさを調整するスクリプト
/// </summary>
public class PlayerShadowController : MonoBehaviour
{

    [SerializeField] private Transform projector;

    private float previousY;

    private float flameDistanceY;

    // Start is called before the first frame update
    void Start()
    {
        previousY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        //ラストフレームと現在のフレームでのプレイヤーのY軸の差異を代入
        flameDistanceY = transform.position.y - previousY;
        projector.position = new Vector3(projector.position.x, projector.position.y - (flameDistanceY * 1.25f), projector.position.z);
        previousY = transform.position.y;
    }
}
