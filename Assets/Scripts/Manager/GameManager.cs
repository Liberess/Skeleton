using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameObject mContainer;

    private static GameManager mInstance;
    public static GameManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mContainer = new GameObject();
                mContainer.name = "GameManager";
                mInstance = mContainer.AddComponent(typeof(GameManager)) as GameManager;
            }

            return mInstance;
        }
    }
    #endregion
    
    private DataManager dataMgr;

    [SerializeField] private GameObject gameOverCanvas;
    
    [SerializeField] private TextMeshProUGUI waveTxt;
    [SerializeField] private TextMeshProUGUI moneyTxt;
    [SerializeField] private TextMeshProUGUI shopMoneyTxt;
    [SerializeField] private TextMeshProUGUI timeTxt;
    [SerializeField] private TextMeshProUGUI remainTxt;

    public UnityAction NextWaveAction;
    public UnityAction GameOverAction;

    [SerializeField] private bool isPlaying = true;
    public bool IsPlaying => isPlaying;
    
    private float waveTime = 0.0f;
    
    [SerializeField] private float originWaveTime = 120.0f;

    private int wave = 1;
    public int Wave => wave;

    private readonly int MaxGold = 999999;

    private int gold = 50;
    public int Gold
    {
        get => gold;
        set
        {
            gold = Mathf.Clamp(value, 0, MaxGold);

            if (gold > 0)
            {
                moneyTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", $"{gold:#,###}");
                shopMoneyTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", $"{gold:#,###}");
            }
            else
            {
                moneyTxt.text = string.Concat("<color=#ffd900>Gold</color> : 0");
                shopMoneyTxt.text = string.Concat("<color=#ffd900>Gold</color> : 0");   
            }            
        }
    }

    public int currentkillCount = 0;
    public float currentPlayTime = 0;

    private DateTime startTime;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;
        else if (mInstance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        dataMgr = DataManager.Instance;
        
        waveTime = originWaveTime;
        ++dataMgr.GameData.waveCount;

        Gold = 50;

        startTime = DateTime.Now;
        
        GameOverAction += GameOver;

        UpdateGameUI();
        
        NextWaveAction += () =>
        {
            ++wave;
            waveTime = originWaveTime;
            ++dataMgr.GameData.waveCount;
            UpdateGameUI();
        };
        
        //AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }

    private void Update()
    {
        if (isPlaying)
        {
            if (waveTime > 0)
            {
                waveTime -= Time.deltaTime;
            }
            else
            {
                waveTime = originWaveTime;
                NextWaveAction?.Invoke();
            }
            
            timeTxt.text = string.Concat("Survive Time : ", string.Format("{0:N2}", waveTime));
        }
    }

    public void GetGold(int value) => Gold += value;

    public void OnClickNext()
    {
        waveTime = originWaveTime;
        NextWaveAction?.Invoke();
    }

    private void UpdateGameUI()
    {
        waveTxt.text = string.Concat("Wave ", wave);
        timeTxt.text = string.Concat("Survive Time : ", string.Format("{0:N2}", waveTime));
        moneyTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", gold);
    }

    public void UpdateRemainZombieUI(int count)
    {
        remainTxt.text = string.Concat("Remain Zombie : ", count);
    }

    private void GameOver()
    {
        DateTime endTime = DateTime.Now;
        TimeSpan timeDif = endTime - startTime;

        currentPlayTime = (float)timeDif.TotalSeconds;

        if (currentPlayTime > dataMgr.GameData.bestPlayTime)
            dataMgr.GameData.bestPlayTime = currentPlayTime;
        
        dataMgr.GameData.totalPlayTime += currentPlayTime;
        dataMgr.GameData.karma += currentkillCount;
        
        ++dataMgr.GameData.deathCount;
        dataMgr.GameData.killCount += currentkillCount;

        gameOverCanvas.SetActive(true);
    }
}
