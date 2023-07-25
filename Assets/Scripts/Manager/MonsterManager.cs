using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }
    private GameManager gameMgr;
    
    [SerializeField, Range(1, 100)] private int monsterInitAmount = 30;
    [SerializeField] private GameObject[] monsterPrefabs;
    [SerializeField] private EntitySO[] monsterSOs;
    private Queue<Monster> monsterQueue = new Queue<Monster>();

    private int spawnCount = 0;
    [SerializeField] private int maxSpawnCount = 100;
    [SerializeField] private Transform[] spawnPoints;
    private Dictionary<EMonsterType, int> spawnWeightDic = new Dictionary<EMonsterType, int>()
    {
        { EMonsterType.Spider, 100},
        { EMonsterType.Test1, 50},
        { EMonsterType.Test2, 1}
    };

    public List<Monster> SpawnedMonsterList { get; } = new List<Monster>();

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
        Initialize(monsterInitAmount);
        gameMgr.NextWaveAction += () => Spawn();
        Spawn();
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
        var zombiePrefab = monsterPrefabs[(int)type];
        var newObj = Instantiate(zombiePrefab, transform.position, Quaternion.identity);
        newObj.name = string.Concat(zombiePrefab.name, "_", index);
        newObj.gameObject.SetActive(false);
        newObj.GetComponent<NavMeshAgent>().enabled = false;
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
    
    private void Spawn()
    {
        spawnCount += gameMgr.Wave * 2;
        if (spawnCount > maxSpawnCount)
            spawnCount = maxSpawnCount;
        gameMgr.UpdateRemainZombieUI(spawnCount);
        
        var picker = new Rito.WeightedRandomPicker<EMonsterType>();
        picker.Add(EMonsterType.Spider, spawnWeightDic[EMonsterType.Spider]);
        picker.Add(EMonsterType.Test1, spawnWeightDic[EMonsterType.Test1]);
        picker.Add(EMonsterType.Test2, spawnWeightDic[EMonsterType.Test2]);

        for (int i = 0; i < spawnCount; i++)
        {
            var pick = picker.GetRandomPick();
            var monster = InstantiateObj(pick);
            var targetPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            var randPos = Utility.GetRandPointOnNavMesh(targetPos, 3f, NavMesh.AllAreas);
            monster.transform.position = randPos;
            monster.GetComponent<NavMeshAgent>().enabled = true;
            monster.SetupEntityData(monsterSOs[(int)pick].entityData);
            monster.EntityData.attackPower = monster.EntityData.attackPower * gameMgr.Wave;
            monster.EntityData.healthPoint = monster.EntityData.healthPoint * gameMgr.Wave;
            monster.DeathAction += () => SpawnedMonsterList.Remove(monster);
            monster.DeathAction += () => gameMgr.UpdateRemainZombieUI(--spawnCount);
            monster.DeathAction += () => ++GameManager.Instance.currentkillCount;
            
            SpawnedMonsterList.Add(monster);
        }

        spawnWeightDic[EMonsterType.Spider] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Spider] - gameMgr.Wave,
                0, 100);
        spawnWeightDic[EMonsterType.Test1] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Test1] + gameMgr.Wave,
                0, 99);
        spawnWeightDic[EMonsterType.Test2] =
            Mathf.Clamp(spawnWeightDic[EMonsterType.Test2] + gameMgr.Wave,
                0, 99);
    }
}
