using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    private Vector3 offset;
    private float changeRotationParcent;
    private float changePositionParcent;
    private float toFinishParcent;
    private float rotateValue = 25;
    private Quaternion changeRotationPosition = Quaternion.Euler(0, -14.1f, -1);


    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - playerTransform.position;
    }

    void LateUpdate()
    {
        if (Locator.i.playerController.CurrentPlayerState == PlayerController.PlayerState.Goal) return;
        if (Locator.i.playerController.CurrentPlayerState == PlayerController.PlayerState.Fail) return;

        if (!Locator.i.playerController.isChangeCameraPosition)
        {
            transform.position = playerTransform.position + offset;
        }
        else
        {
            //ゴール加速時のカメラ視点の切り替え
            transform.position = playerTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + -1f, transform.position.y + -7f, transform.position.z - 1f),
           changePositionParcent);
            transform.rotation = Quaternion.Slerp(transform.rotation, changeRotationPosition, changeRotationParcent);
        }
    }

    void Update()
    {
        if (Locator.i.playerController.isChangeCameraPosition)
        {
            changeRotationParcent += 0.2f * Time.deltaTime;
            changePositionParcent += 0.625f * Time.deltaTime;
        }
        if (changeRotationParcent > 1) changeRotationParcent = 1;
        if (changePositionParcent > 0.5) changePositionParcent = 0.5f;
        if (Locator.i.playerController.CurrentPlayerState == PlayerController.PlayerState.Goal) GoalCameraMove();
    }

    void GoalCameraMove()
    {
        //加速していないでゴールした際に、カメラ視点のy軸をプレイヤーのy軸近くまで移動させると同時にカメラがプレイヤーの周りを回転するように変更
        if (!Locator.i.playerController.isChangeCameraPosition)
        {
            toFinishParcent += 0.05f * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, playerTransform.position.y + 1.5f, transform.position.z), toFinishParcent);
        }

        //ゴール時カメラ視点の回転処理　　
        if (transform.position.z >= playerTransform.position.z)
        {
            rotateValue *= -1;
        }
        transform.RotateAround(playerTransform.position, Vector3.up, rotateValue * Time.deltaTime);
    }
}
