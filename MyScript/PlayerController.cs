using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;
    //ブロック発生ポイント
    [SerializeField] private Transform blockPoint;
    [SerializeField] private Animator animator;
    //通常時のキャラクターの色
    [SerializeField] private Material usualMaterial;
    //ノックバック時のキャラクターの色
    [SerializeField] private Material knockbackMaterial;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private ParticleSystem accelateEffect;

    public enum PlayerState
    {
        Idle,
        Move,
        Fall,
        Goal,
        Fail,
        KnockBack,
        Climb,
        Accelate,
    }
    public PlayerState CurrentPlayerState { get; set; }
    //ブロック管理リスト
    private List<BlockController> blockList = new List<BlockController>();
    //クリア時一番上のブロックポジション
    private Vector3 finalPosition = new Vector3(0, 14.6f, 47.9f);
    //階段生成禁止ポジション
    private Vector3 cannotStairCreatePosition = new Vector3(0, 14, 47.5f);
    //ブロック生成ポジション
    private Vector3 blockPlacement = new Vector3(17, 0, 0.3f);
    //移動スピード
    private Vector3 moveSpeed = new Vector3(0, 0, 3.3f);
    //ブロックのスケール
    private Vector3 childLocalScale = new Vector3(16, 0.8f, 0.3f);
    //ブロック生成座標に使う変数
    private float xPositionDistance = 10;
    private float zPositionDistance = 0.3f;
    private float inputTime = 0;
    //入力を許可する値の最小値
    private float canInputValue = 0.035f;
    private float firstAccelatePositionZ = 32;
    //ステージとプレイヤーが接触しているかどうかのフラグ
    private bool isTouchStage = false;
    //カメラ視点を変えるフラグ
    public bool isChangeCameraPosition { get; set; } = false;

    //ブロック破壊時の親ポジション　インスぺクターから参照でき、外部からの変更を避けるためシリアライズにしプロパティをつけました
    [SerializeField] private Transform breakParent;
    public Transform BreakParent
    {
        get { return breakParent; }
        private set { breakParent = value; }
    }
    //テキスト生成位置の親ポジション　インスぺクターから参照でき、外部からの変更を避けるためシリアライズにしプロパティをつけました
    [SerializeField] private Transform textParent;
    public Transform TextParent
    {
        get { return textParent; }
        private set { textParent = value; }
    }
    //使わなくなったブロックの親ポジション　インスぺクターから参照でき、外部からの変更を避けるためシリアライズにしプロパティをつけました
    [SerializeField] private Transform blockParent;
    public Transform BlockParent
    {
        get { return blockParent; }
        private set { blockParent = value; }
    }

    void OnTriggerEnter(Collider other)
    {
        //ブロック検知をBlockControllerからではなく、PlayerControllerで実施
        if (other.CompareTag("Block"))
        {
            TextCreator();
            StackBlock(other.transform);
            //ブロックのタグを変えて、Triggerが2回呼ばれないようにする
            other.tag = "Collected";
        }
        else if (other.CompareTag("Finish"))
        {
            CurrentPlayerState = PlayerState.Goal;
        }
        else if (other.CompareTag("EffectRun"))
        {
            accelateEffect.gameObject.SetActive(true);
            isChangeCameraPosition = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            //blockList.Countが0になったら失敗演出へ
            if (blockList.Count == 0)
            {
                CurrentPlayerState = PlayerState.Fail;
                return;
            }
            BreakBlock();

            //KnockBack実装をAddForceに変更
            CurrentPlayerState = PlayerState.KnockBack;
            rigidBody.velocity = Vector3.zero;
            animator.SetBool("Fall", false);
            animator.SetBool("Move", true);
            rigidBody.AddForce(new Vector3(0, 9f, -1.25f), ForceMode.Impulse);
            Invoke(nameof(EndKnockBack), 0.8f);

        }
        //ステージとプレイヤーが接触した時
        if (collision.gameObject.CompareTag("Stage"))
        {
            isTouchStage = true;
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        if (!collisionInfo.gameObject.CompareTag("Stage")) return;
        isTouchStage = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentPlayerState = PlayerState.Idle;
        animator.SetBool("Move", false);
    }

    // Update is called once per frame
    void Update()
    {
        //毎フレーム時間をinputTimeに代入　
        inputTime += Time.deltaTime;
        switch (CurrentPlayerState)
        {
            case PlayerState.Idle:
                if (Input.GetMouseButton(0)) CurrentPlayerState = PlayerState.Move;
                break;
            case PlayerState.Goal:
                accelateEffect.gameObject.SetActive(false);
                GoalAnimation();
                break;
            case PlayerState.Fail:
                FailAnimation();
                break;
        }
        //早期リターン
        if (CurrentPlayerState == PlayerState.Goal) return;
        if (CurrentPlayerState == PlayerState.Fail) return;

        //この条件を超えたら強制的にFallにし、処理を終了
        if (transform.position.y > cannotStairCreatePosition.y && transform.position.z >= cannotStairCreatePosition.z)
        {
            CurrentPlayerState = PlayerState.Fall;
            return;
        }
        //　State管理
        StateController();
    }


    void FixedUpdate()
    {
        //移動に関しては、RigidBodyを使っているので、移動を伴うState処理に関しては、FixedUpdateで呼ぶようにしました
        switch (CurrentPlayerState)
        {
            case PlayerState.Move:
                MoveAnimation();
                break;
            case PlayerState.Fall:
                rigidBody.isKinematic = false;
                accelateEffect.gameObject.SetActive(false);
                FallAnimation();
                break;
            case PlayerState.Climb:
                rigidBody.isKinematic = false;
                MoveAnimation();
                break;
            case PlayerState.KnockBack:
                skinnedMeshRenderer.material.color = knockbackMaterial.color;
                if (rigidBody.velocity.y < 0) Physics.gravity = new Vector3(0, -3, 0);
                else Physics.gravity = new Vector3(0, -17f, 0);
                break;
            case PlayerState.Accelate:
                MoveAnimation();
                break;
        }
    }

    //State判定　
    void StateController()
    {
        //Idle時・KnockBack時はリターン
        if (CurrentPlayerState == PlayerState.KnockBack) return;
        if (CurrentPlayerState == PlayerState.Idle) return;

        //Inputを離した瞬間＋地面に触れていない＋Move状態じゃない時or加速中に手持ちブロックがなくなった時
        if (Input.GetMouseButtonUp(0) && !isTouchStage && CurrentPlayerState != PlayerState.Move
        || CurrentPlayerState == PlayerState.Accelate && blockList.Count < 1) CurrentPlayerState = PlayerState.Fall;
        //地面に触れている
        if (isTouchStage) CurrentPlayerState = PlayerState.Move;

        //blockList.Countが１以上の時、inputTimeがcanInputValueを超えた時、階段を生成させる処理へ
        // このゲームは30FPSを基準にして作っているので、階段生成のスピードは30FPSに合わせたい
        // FPSが変わった際も、同じ速度で階段が生成されるように、canInputValueに30FPSの毎フレーム時間を代入し、その値を超えた時のみ階段生成の関数へ
        if (Input.GetMouseButton(0) && blockList.Count >= 1 && inputTime > canInputValue)
        {
            StairController();
            inputTime = 0;
            //プレイヤーが加速開始ポジションより前
            if (transform.position.z < firstAccelatePositionZ) CurrentPlayerState = PlayerState.Climb;
            //プレイヤーが加速開始ポジションを超えている
            else if (transform.position.z > firstAccelatePositionZ) CurrentPlayerState = PlayerState.Accelate;
        }
    }

    void EndKnockBack()
    {
        CurrentPlayerState = PlayerState.Fall;
        rigidBody.velocity = Vector3.zero;
        skinnedMeshRenderer.material.color = usualMaterial.color;
    }

    void FallAnimation()
    {
        Physics.gravity = new Vector3(0, -5f, 0);
        if (transform.position.y > finalPosition.y && transform.position.z >= finalPosition.z) moveSpeed.z = 0;
        else moveSpeed.z = 2.7f;
        moveSpeed.y = 0f;

        animator.SetBool("Move", false);
        animator.SetBool("Fall", true);
        rigidBody.MovePosition(rigidBody.position + moveSpeed * Time.fixedDeltaTime);
    }
    void MoveAnimation()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);

        switch (CurrentPlayerState)
        {
            case PlayerState.Move:
                moveSpeed.z = 3.3f;
                moveSpeed.y = 0;
                break;
            case PlayerState.Climb:
                moveSpeed.z = 3.3f;
                moveSpeed.y = 1.5f;
                break;
            case PlayerState.Accelate:
                moveSpeed.y = 7f;
                moveSpeed.z = 7f;
                break;
        }
        //アニメーションを実行する処理
        animator.SetBool("Fall", false);
        animator.SetBool("Move", true);
        rigidBody.MovePosition(rigidBody.position + moveSpeed * Time.fixedDeltaTime);
    }
    //　ブロックを破壊する演出
    void BreakBlock()
    {
        //ブロックが十個以上ある時はランダムで破壊する数を決める
        int breakCount;
        if (blockList.Count >= 10)
        {
            breakCount = Random.Range(8, 10);
        }
        else
        {
            breakCount = blockList.Count;
        }

        for (int i = 0; i < breakCount; i++)
        {
            if (blockList.Count % 3 == 1 && blockList.Count > 1)
            {
                xPositionDistance -= blockPlacement.x;
            }
            BlockController lastobj = blockList[blockList.Count - 1];
            blockList.Remove(lastobj);
            lastobj.Break();
        }
    }
    //失敗演出の処理
    void FailAnimation()
    {
        rigidBody.AddForce(0, -100, -10);
        animator.speed = 0;
        rigidBody.constraints = RigidbodyConstraints.None;
        //失敗画面
        if (Variables.screenState != ScreenState.Game) return;
        Variables.screenState = ScreenState.Failed;
    }

    void GoalAnimation()
    {
        OpenClearScreen();
        animator.SetBool("Move", false);
        animator.SetBool("Fall", false);
        animator.SetTrigger("Goal");
    }
    //クリア画面
    void OpenClearScreen()
    {
        if (Variables.screenState != ScreenState.Game) return;
        Variables.screenState = ScreenState.Clear;
    }

    // 箱にブロックを積み上げる処理
    void StackBlock(Transform transform)
    {
        BlockController blockController = transform.GetComponent<BlockController>();
        blockList.Add(blockController);

        //3の倍数の時ブロックの段を一つあげるように設定　ブロック生成位置はあまりの数で遷移するように設定
        if (blockList.Count % 3 == 1 && blockList.Count > 1)
        {
            zPositionDistance = blockPlacement.z;
            xPositionDistance += blockPlacement.x;
        }
        else if (blockList.Count % 3 == 2)
        {
            zPositionDistance = 0;
        }
        else if (blockList.Count % 3 == 0)
        {
            zPositionDistance = -blockPlacement.z;
        }

        transform.SetParent(blockPoint);
        transform.localScale = childLocalScale;
        transform.localPosition = new Vector3(xPositionDistance, 0, zPositionDistance);
        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    //テキストを生成
    void TextCreator()
    {
        GameObject _poolObject = Locator.i.objectPooling.GetGameObject();
        if (_poolObject == null) return;
        _poolObject.SetActive(true);
    }

    //階段削除・生成を管理する処理
    void StairController()
    {
        //階段削除処理
        if (blockList.Count % 3 == 1 && blockList.Count > 1)
        {
            xPositionDistance -= blockPlacement.x;
        }
        BlockController lastobj = blockList[blockList.Count - 1];
        blockList.Remove(lastobj);
        lastobj.StairCreator();
    }
}
