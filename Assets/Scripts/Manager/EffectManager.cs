using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField, Range(1, 100)] private int initAmount = 30;

    private Dictionary<EEffectType, Queue<GameObject>> queDic =
        new Dictionary<EEffectType, Queue<GameObject>>();
    
    private Dictionary<EEffectType, GameObject> quePrefabDic =
        new Dictionary<EEffectType, GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        queDic.Clear();
        quePrefabDic.Clear();
        
        quePrefabDic.Add(EEffectType.FireBall, fireBallPrefab);
        quePrefabDic.Add(EEffectType.Explosion, explosionPrefab);
        //quePrefabDic.Add(EEffectType.Blood, bloodPrefab);

        Initialize(EEffectType.FireBall, 2);
        Initialize(EEffectType.Explosion, 2);
    }
    
    #region Object Pooling
    private void Initialize(EEffectType type, int initCount)
    {
        queDic.Add(type, new Queue<GameObject>());

        for (int i = 0; i < initCount; i++)
            queDic[type].Enqueue(CreateNewObj(type, i));
    }

    private GameObject CreateNewObj(EEffectType type, int index = 0)
    {
        if (!quePrefabDic.ContainsKey(type))
        {
            Debug.Log("해당 " + type + " 타입의 Key가 존재하지 않음");
            return null;
        }

        if(!quePrefabDic[type])
        {
            Debug.Log("해당 " + type + " 타입의 Value가 존재하지 않음");
            return null;
        }
        
        var newObj = Instantiate(quePrefabDic[type].gameObject, transform.position, Quaternion.identity);
        newObj.name = type.ToString() + index;
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public GameObject GetObj(EEffectType type)
    {
        if (Instance.queDic[type].Count > 0)
        {
            var obj = Instance.queDic[type].Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }

        var newObj = Instance.CreateNewObj(type);
        newObj.gameObject.SetActive(true);
        newObj.transform.SetParent(null);
        return newObj;
    }

    public GameObject InstantiateObj(EEffectType type) => GetObj(type);
    
    public void ReturnObj(EEffectType type, GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.queDic[type].Enqueue(obj);
    }

    public void ReturnObj(EEffectType type, GameObject obj, float delayTime = 0.0f)
    {
        StartCoroutine(ReturnObjCo(type, obj, delayTime));
    }

    private IEnumerator ReturnObjCo(EEffectType type, GameObject obj, float delayTime = 0.0f)
    {
        yield return new WaitForSeconds(delayTime);
        
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.queDic[type].Enqueue(obj);
    }
    #endregion
}
