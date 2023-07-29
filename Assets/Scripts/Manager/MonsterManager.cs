using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }
    private GameManager gameMgr;
    private DataManager dataMgr;
    
    [HorizontalLine(color: EColor.Red), SerializeField, Range(1, 100)]
    private int monsterInitAmount = 30;
    
    [SerializeField]
    private GameObject[] monsterPrefabs;
    
    [SerializeField]
    private EntitySO[] monsterSOs;
    
    private Queue<Monster> monsterQueue = new Queue<Monster>();

    private int curSpawnCount = 0;
    
    public int StageSpawnCount { get; private set; }
    
    [ShowNonSerializedField]
    private float spawnCycleTime = 5.0f;
    
    [SerializeField, Range(0.0f, 60.0f)]
    private float originSpawnCycleTime = 5.0f;
    
    [SerializeField]
    private int maxSpawnCount = 100;

    private Dictionary<EMonsterType, int> spawnWeightDic = new Dictionary<EMonsterType, int>()
    {
        { EMonsterType.Spider, 100},
        { EMonsterType.Test1, 50},
        { EMonsterType.Test2, 1}
    };

    [SerializeField]
    private List<Monster> spawnedMonsterList = new List<Monster>();

    public List<Monster> SpawnedMonsterList
    {
        get => spawnedMonsterList;
        private set => spawnedMonsterList = value;
    }

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

        spawnCycleTime = originSpawnCycleTime;
        
        Initialize(monsterInitAmount);
        
        gameMgr.NextWaveAction += () => StartCoroutine(SpawnCo());
        gameMgr.GameOverAction += () =>
        {
            for (int i = spawnedMonsterList.Count - 1; i >= 0; i--)
            {
                if (spawnedMonsterList[i] != null)
                    ReturnObj(spawnedMonsterList[i]);
                spawnedMonsterList.RemoveAt(i);
            }

            gameMgr.UpdateRemainMonsterUI(0);
        };
    }
    
    #region Object Pooling
    private void Initialize(int initCount)
    {
        for (int i = 0; i < monsterPrefabs.Length; i++)
        {
            for (int j = 0; j < initCount; j++)
                monsterQueue.Enqueue(CreateNewObj((EMonsterType)i, j));
        }
    }

    private Monster CreateNewObj(EMonsterType type, int index = 0)
    {
        var monsterPrefab = monsterPrefabs[(int)type];
        var newObj = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
        newObj.name = string.Concat(monsterPrefab.name, "_", index);
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj.GetComponent<Monster>();
    }

    private Monster GetObj(EMonsterType type)
    {
        if (monsterQueue.Count > 0)
        {
            var obj = monsterQueue.Dequeue();
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
    
    public void ReturnObj(Monster obj, float delay = 0.0f)
    {
        StartCoroutine(ReturnObjCo(obj, delay));
    }

    private IEnumerator ReturnObjCo(Monster obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        monsterQueue.Enqueue(obj);
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
        
        /*var picker = new Rito.WeightedRandomPicker<EMonsterType>();
        picker.Add(EMonsterType.Spider, spawnWeightDic[EMonsterType.Spider]);
        picker.Add(EMonsterType.Test1, spawnWeightDic[EMonsterType.Test1]);
        picker.Add(EMonsterType.Test2, spawnWeightDic[EMonsterType.Test2]);*/

        WaitForSeconds spawnDelay = new WaitForSeconds(spawnCycleTime);

        for (int i = 0; i < curSpawnCount; i++)
        {
            yield return spawnDelay;
            
            //var pick = picker.GetRandomPick();
            var monster = InstantiateObj(EMonsterType.Spider);
            var randPos = Utility.GetRandPointOnNavMesh(Vector3.zero, 50f);
            monster.transform.position = randPos;

            float increaseValue = dataMgr.GameData.stageCount * dataMgr.GetStageNumber(EStageNumberType.Main) * 0.4f;
            monster.SetupEntityData(monsterSOs[(int)EMonsterType.Spider].entityData, increaseValue);

            monster.DeathAction += () =>
            {
                SpawnedMonsterList.Remove(monster);
                ++gameMgr.curkillCount;
                ++dataMgr.GameData.killCount;
                gameMgr.UpdateRemainMonsterUI(--curSpawnCount);
                StartCoroutine(ReturnObjCo(monster, 1.0f));

                if (SpawnedMonsterList.Count == 0)
                    gameMgr.StartCoroutine(gameMgr.InvokeNextWaveCo());
            };

            SpawnedMonsterList.Add(monster);
        }

        /*
        spawnWeightDic[EMonsterType.Spider] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Spider] - dataMgr.GameData.stageCount,
                0, 100);
        spawnWeightDic[EMonsterType.Test1] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Test1] + dataMgr.GameData.stageCount,
                0, 99);
        spawnWeightDic[EMonsterType.Test2] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Test2] + dataMgr.GameData.stageCount,
                0, 99);
                */

        spawnCycleTime = Mathf.Clamp(originSpawnCycleTime - dataMgr.GameData.stageCount * 0.01f, 0.2f, originSpawnCycleTime);
        
        yield return null;
    }
}
