using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    //移動時間
    private float moveTime = 0;
    //移動時スタートポジション
    private Vector3 startTextPosition = new Vector3(0.075f, 0.85f, -0.85f);
    //移動時エンドポジション
    private Vector3 endTextPosition = new Vector3(0.075f, 0.85f, -0.4f);

    private TextMeshPro textMeshPro;

    void OnEnable()
    {
        transform.SetParent(Locator.i.playerController.TextParent);
        transform.localScale = new Vector3(1.5f, 1.35f, 1);
        transform.localRotation = Quaternion.Euler(0, -20, 0);
    }

    void OnDisable()
    {
        moveTime = 0;
    }

    void Start()
    {
        textMeshPro = gameObject.GetComponentInChildren<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf == true)
        {
            moveTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startTextPosition, endTextPosition, moveTime);
            textMeshPro.color = Color.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), moveTime);
        }

        if (moveTime >= 1)
        {
            gameObject.SetActive(false);
        }
    }
}
