using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// +1テキストのオブジェクトプーリング
/// </summary>
public class ObjectPooling : MonoBehaviour
{

    private List<GameObject> poolList = new List<GameObject>();
    [SerializeField] private GameObject poolObject;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(poolObject, transform);
            obj.SetActive(false);
            poolList.Add(obj);
        }
    }

    public GameObject GetGameObject()
    {
        for (int i = 0; i < poolList.Count; i++)
        {
            if (poolList[i].activeInHierarchy == false)
            {
                return poolList[i];
            }
        }
        return null;
    }
}
