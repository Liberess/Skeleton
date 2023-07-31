using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }
    private GameManager gameMgr;
    private DataManager dataMgr;
    
    [HorizontalLine(color: EColor.Red), BoxGroup("# Object Pooling"), SerializeField, Range(1, 100)]
    private int monsterInitAmount = 30;
    
    [BoxGroup("# Object Pooling"), SerializeField]
    private GameObject[] monsterPrefabs;
    
    [BoxGroup("# Object Pooling"), SerializeField]
    private EntitySO[] monsterSOs;

    private Dictionary<EMonsterType, Queue<Monster>> monsterQueDic = new Dictionary<EMonsterType, Queue<Monster>>();
    private Dictionary<EMonsterType, GameObject> monsterQuePrefabDic = new Dictionary<EMonsterType, GameObject>();
    
    private int curSpawnCount = 0;
    
    public int StageSpawnCount { get; private set; }
    
    [HorizontalLine(color: EColor.Orange), BoxGroup("# Spawn Setting"), ShowNonSerializedField]
    private float spawnCycleTime = 5.0f;
    
    [BoxGroup("# Spawn Setting"), SerializeField, Range(0.0f, 60.0f)]
    private float originSpawnCycleTime = 5.0f;
    
    [BoxGroup("# Spawn Setting"), SerializeField]
    private int maxSpawnCount = 100;

    private Dictionary<EMonsterType, int> spawnWeightDic = new Dictionary<EMonsterType, int>()
    {
        { EMonsterType.Spider, 100},
        { EMonsterType.RedSpider, 50},
        { EMonsterType.YellowSpider, 30},
        { EMonsterType.GreenSpider, 15},
        { EMonsterType.BlueSpider, 1}
    };

    public List<Monster> SpawnedMonsterList { get; private set; } = new List<Monster>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        gameMgr = GameManager.Instance;
        dataMgr = DataManager.Instance;
        
        Initialize();
        
        gameMgr.NextWaveAction += () => StartCoroutine(SpawnCo());
        gameMgr.GameOverAction += OnGameOver;
    }

    private void OnGameOver()
    {
        for (int i = SpawnedMonsterList.Count - 1; i >= 0; i--)
        {
            if (SpawnedMonsterList[i] != null)
                ReturnObj(SpawnedMonsterList[i].MonsterType, SpawnedMonsterList[i]);
            SpawnedMonsterList.RemoveAt(i);
        }

        gameMgr.UpdateRemainMonsterUI(0);
    }
    
    #region Object Pooling

    private void Initialize()
    {
        spawnCycleTime = originSpawnCycleTime;

        foreach (EMonsterType monsterType in Enum.GetValues(typeof(EMonsterType)))
        {
            monsterQuePrefabDic.Add(monsterType, monsterPrefabs[(int)monsterType]);
            
            if(monsterType == EMonsterType.BigSpider)
                continue;
            
            // 중간 보스는 따로 초기화 한다.
            InitializeQueue(monsterType, monsterInitAmount);
        }

        // 중간 보스는 메인 스테이지 단위로 스폰되기에 수가 적어도 괜찮다.
        InitializeQueue(EMonsterType.BigSpider, monsterInitAmount / monsterInitAmount);
    }
        
    private void InitializeQueue(EMonsterType monsterType, int initCount)
    {
        monsterQueDic.Add(monsterType, new Queue<Monster>());

        for (int i = 0; i < initCount; i++)
            monsterQueDic[monsterType].Enqueue(CreateNewObj(monsterType, i));
    }

    private Monster CreateNewObj(EMonsterType type, int index = 0)
    {
        if (!monsterQuePrefabDic.ContainsKey(type))
            throw new Exception($"해당 {type}의 Key가 존재하지 않습니다.");

        if(!monsterQuePrefabDic[type])
            throw new Exception($"해당 {type}의 Value가 존재하지 않습니다.");

        var newObj = Instantiate(monsterQuePrefabDic[type], transform.position, Quaternion.identity);
        newObj.name = string.Concat(monsterQuePrefabDic[type].name, "_", index);
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj.GetComponent<Monster>();
    }

    private Monster GetObj(EMonsterType type)
    {
        if (monsterQueDic[type].Count > 0)
        {
            var obj = monsterQueDic[type].Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = CreateNewObj(type);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    private Monster InstantiateObj(EMonsterType type) => GetObj(type);
    
    public void ReturnObj(EMonsterType type, Monster obj, float delay = 0.0f)
    {
        StartCoroutine(ReturnObjCo(type, obj, delay));
    }

    private IEnumerator ReturnObjCo(EMonsterType type, Monster obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        monsterQueDic[type].Enqueue(obj);
    }
    #endregion

    public void Spawn() => StartCoroutine(SpawnCo());

    private IEnumerator SpawnCo()
    {
        curSpawnCount = dataMgr.GameData.stageCount * 2;
        StageSpawnCount = curSpawnCount;
        if (curSpawnCount > maxSpawnCount)
            curSpawnCount = maxSpawnCount;
        gameMgr.UpdateRemainMonsterUI(curSpawnCount);

        var picker = new Rito.WeightedRandomPicker<EMonsterType>();
        foreach (EMonsterType monsterType in Enum.GetValues(typeof(EMonsterType)))
        {
            if (monsterType != EMonsterType.BigSpider)
                picker.Add(monsterType, spawnWeightDic[monsterType]);
        }

        WaitForSeconds spawnDelay = new WaitForSeconds(spawnCycleTime);

        for (int i = 0; i < curSpawnCount; i++)
        {
            yield return spawnDelay;
            
            EMonsterType monsterTypePick = picker.GetRandomPick();
            var monster = InstantiateObj(monsterTypePick);
            var randPos = Utility.GetRandPointOnNavMesh(Vector3.zero, 50f);
            monster.transform.position = randPos;

            float increaseValue = dataMgr.GameData.stageCount * dataMgr.GetStageNumber(EStageNumberType.Main) * 0.4f;
            monster.SetupEntityData(monsterTypePick, monsterSOs[(int)monsterTypePick].entityData, increaseValue);

            monster.DeathAction += () =>
            {
                SpawnedMonsterList.Remove(monster);
                ++gameMgr.curkillCount;
                ++dataMgr.GameData.killCount;
                gameMgr.UpdateRemainMonsterUI(--curSpawnCount);
                StartCoroutine(ReturnObjCo(monsterTypePick, monster, 1.0f));

                if (SpawnedMonsterList.Count == 0)
                    gameMgr.StartCoroutine(gameMgr.InvokeNextWaveCo());
            };

            SpawnedMonsterList.Add(monster);
        }

        foreach (EMonsterType monsterType in Enum.GetValues(typeof(EMonsterType)))
        {
            if (spawnWeightDic.TryGetValue(monsterType, out var spawnWeight))
            {
                int currentSpawnWeight = spawnWeight;

                int newSpawnWeight = 0;
                if(monsterType == EMonsterType.Spider)
                    newSpawnWeight = currentSpawnWeight - dataMgr.GameData.stageCount;
                else
                    newSpawnWeight = currentSpawnWeight + dataMgr.GameData.stageCount;

                spawnWeightDic[monsterType] = Mathf.Clamp(newSpawnWeight, 1, 99);
            }
        }

        spawnCycleTime = Mathf.Clamp(originSpawnCycleTime - dataMgr.GameData.stageCount * 0.01f, 0.2f, originSpawnCycleTime);
        
        yield return null;
    }
}
