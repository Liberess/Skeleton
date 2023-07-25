using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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
    
    [SerializeField] private TextMeshProUGUI stageTxt;
    [SerializeField] private TextMeshProUGUI karmaTxt;
    [SerializeField] private TextMeshProUGUI shopKarmaTxt;
    [SerializeField] private TextMeshProUGUI remainTxt;

    public UnityAction NextWaveAction;
    public UnityAction GameOverAction;

    [SerializeField] private bool isPlaying = true;
    public bool IsPlaying => isPlaying;
 
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
                karmaTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", $"{gold:#,###}");
                shopKarmaTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", $"{gold:#,###}");
            }
            else
            {
                karmaTxt.text = string.Concat("<color=#ffd900>Gold</color> : 0");
                shopKarmaTxt.text = string.Concat("<color=#ffd900>Gold</color> : 0");   
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

        ++dataMgr.GameData.waveCount;

        Gold = 50;

        startTime = DateTime.Now;
        
        GameOverAction += GameOver;

        UpdateGameUI();
        
        NextWaveAction += () =>
        {
            ++wave;
            ++dataMgr.GameData.waveCount;
            UpdateGameUI();
        };
        
        //AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }

    public void GetGold(int value) => Gold += value;

    public void OnClickNext()
    {
        NextWaveAction?.Invoke();
    }

    private void UpdateGameUI()
    {
        stageTxt.text = string.Concat("Wave ", wave);
        karmaTxt.text = string.Concat("<color=#ffd900>Gold</color> : ", gold);
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
