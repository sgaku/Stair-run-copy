using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ブロックを管理するスクリプト
/// </summary>
public class BlockController : MonoBehaviour
{

    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Collider boxCollider;
    private float toGravityTime;
    //階段生成時のローカルスケール
    private Vector3 stairLocalScale;

    void Start()
    {
        toGravityTime = 0.6f;
        stairLocalScale = new Vector3(0.9f, 0.15f, 0.2f);
    }

    //ブロック破壊演出
    public void Break()
    {
        //ブロックのレイヤーを変え、プレイヤーと接触反応が起きないように設定
        gameObject.layer = 8;
        transform.parent = Locator.i.playerController.BreakParent;
        transform.localPosition = new Vector3(0, 0, 0);
        boxCollider.isTrigger = false;
        rigidBody.constraints = RigidbodyConstraints.None;
        rigidBody.isKinematic = false;
        Invoke(nameof(ChangeParent), 1f);
    }

    void ChangeParent()
    {
        transform.parent = Locator.i.playerController.BlockParent;
    }

    //階段生成処理　
    public void StairCreator()
    {
        transform.parent = Locator.i.playerController.BlockParent;
        if (Locator.i.playerController.CurrentPlayerState == PlayerController.PlayerState.Accelate) transform.position = new Vector3(0, Locator.i.playerController.transform.position.y + 0.085f, Locator.i.playerController.transform.position.z + 0.075f);
        else transform.position = new Vector3(0, Locator.i.playerController.transform.position.y + 0.075f, Locator.i.playerController.transform.position.z + 0.075f);
        boxCollider.isTrigger = false;
        transform.localScale = stairLocalScale;
        transform.localEulerAngles = new Vector3(0, 0, 0);
        Invoke(nameof(ChangeStairGravity), toGravityTime);
    }
    //重力落下処理
    public void ChangeStairGravity()
    {
        rigidBody.isKinematic = false;
    }
}
