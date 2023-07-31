using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private UIManager uiMgr;
    private DataManager dataMgr;
    private PlayerController playerCtrl;
    
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject gameOverCanvas;

    [SerializeField] private TextMeshProUGUI stageTxt;
    [SerializeField] private TextMeshProUGUI remainTxt;
    [SerializeField] private Image remainFillImg;
    [SerializeField] private Slider remainBar;

    [SerializeField] private TextMeshProUGUI lvTxt;
    [SerializeField] private Slider expSlider;

    public UnityAction NextWaveAction;
    public UnityAction GameOverAction;

    [ShowNonSerializedField] private bool isPlaying = false;
    public bool IsPlaying => isPlaying;
    
    [ShowNonSerializedField] private EGameState gameState;
    public EGameState GameState => gameState;

    public float Exp
    {
        get => dataMgr.GameData.exp;
        set
        {
            dataMgr.GameData.exp = Mathf.Clamp(value, 0.0f, 100.0f);

            if (dataMgr.GameData.exp >= dataMgr.GameData.needsExp)
            {
                AudioManager.Instance.PlaySFX(ESFXName.LevelUp);
                
                ++dataMgr.GameData.level;
                dataMgr.GameData.exp = Mathf.Clamp(dataMgr.GameData.exp - dataMgr.GameData.needsExp, 0.0f,
                    dataMgr.GameData.needsExp);

                lvTxt.text = dataMgr.GameData.level.ToString();

                // 맨 뒤의 계수는 10만 테이블당 1을 뜻함
                dataMgr.GameData.needsExp = Mathf.Pow(((dataMgr.GameData.level - 1) * 50 / 49), 2.5f) * 10;
            }

            expSlider.value = dataMgr.GameData.exp / dataMgr.GameData.needsExp;
        }
    }

    public int Gold
    {
        get => dataMgr.GameData.gold;
        set
        {
            dataMgr.GameData.gold = Mathf.Clamp(value, 0, dataMgr.GameData.MaxGoodsAmount);
            uiMgr.InvokeCurrencyUI(ECurrencyType.GD, dataMgr.GameData.gold);
        }
    }

    public int Karma
    {
        get => dataMgr.GameData.karma;
        set
        {
            dataMgr.GameData.karma = Mathf.Clamp(value, 0, dataMgr.GameData.MaxGoodsAmount);
            uiMgr.InvokeCurrencyUI(ECurrencyType.KM, dataMgr.GameData.karma);
        }
    }

    // 현재 Stage에서 잡은 Monster의 수
    public int curkillCount = 0;
    
    // 죽기 전까지 플레이한 시간
    private float curPlayTime = 0;

    // 게임을 시작한 시간
    private DateTime startTime;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        Application.targetFrameRate = 60;

        GameOverAction += GameOver;
        NextWaveAction += NextStage;
    }

    private void Start()
    {
        uiMgr = UIManager.Instance;
        dataMgr = DataManager.Instance;
        
        playerCtrl = FindObjectOfType<PlayerController>();
        
        expSlider.maxValue = 1.0f;
        remainBar.maxValue = 1.0f;

        Gold = 50000;
        Karma = 50000;
        
        mainCanvas.SetActive(true);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!isPlaying && Input.anyKeyDown)
            StartGame();
#else
        if (!isPlaying && Input.touchCount > 0)
            StartGame();
#endif
    }

    public void StartGame()
    {
        if (gameState == EGameState.Main && !dataMgr.GameData.isNewGame)
            dataMgr.CalculateOfflineTime();
        
        gameState = EGameState.InGame;
        
        isPlaying = true;

        startTime = DateTime.Now;

        curkillCount = 0;
        remainBar.value = 0;

        UpdateGameUI();
        UpdateRemainMonsterUI(0);

        playerCtrl.gameObject.SetActive(false);
        playerCtrl.gameObject.SetActive(true);
  
        uiMgr.InitializedProperty();

        MonsterManager.Instance.Spawn();
        
        mainCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        uiMgr.OpenPanel();
        
        AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }

    public void SetGold(int value) => Gold += value;
    public void SetKarma(int value) => Karma += value;

    public IEnumerator InvokeNextWaveCo(float delay = 0.0f)
    {
        /*curkillCount = 0;
        remainBar.value = 0;*/
        
        yield return new WaitForSeconds(delay);

        NextWaveAction?.Invoke();
    }

    public void NextStage()
    {
        string[] subStr = dataMgr.GameData.stageStr.Split('-');
        int mainStageNum = int.Parse(subStr[0]);
        int subStageNum = int.Parse(subStr[1]);

        ++subStageNum;
        if(subStageNum > 10)
        {
            ++mainStageNum;
            subStageNum = 1;
        }
        
        curkillCount = 0;
        remainBar.value = 0;
        
        ++dataMgr.GameData.stageCount;
        
        dataMgr.GameData.karma += mainStageNum;

        dataMgr.GameData.stageStr = string.Concat(mainStageNum, '-', subStageNum);
        stageTxt.text = dataMgr.GameData.stageStr;
        remainFillImg.fillAmount = 1.0f;
    }

    private void UpdateGameUI()
    {
        stageTxt.text = dataMgr.GameData.stageStr;
        lvTxt.text = dataMgr.GameData.level.ToString();
        expSlider.value = dataMgr.GameData.exp / dataMgr.GameData.needsExp;
    }

    public void UpdateRemainMonsterUI(int count)
    {
        remainTxt.text = count.ToString();
        remainFillImg.fillAmount = ((float)count / MonsterManager.Instance.StageSpawnCount);
        
        if(curkillCount > 0)
            remainBar.value = (float)curkillCount / MonsterManager.Instance.StageSpawnCount;
    }

    private void GameOver()
    {
        isPlaying = false;
        
        DateTime endTime = DateTime.Now;
        TimeSpan timeDif = endTime - startTime;
        curPlayTime = (float)timeDif.TotalSeconds;
        dataMgr.GameData.totalPlayTime += curPlayTime;

        ++dataMgr.GameData.deathCount;

        gameCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        
        AudioManager.Instance.StopBGM();
    }

    public void RetryGame() => StartGame();

    public void QuitGame()
    {
        if (isPlaying)
        {
            isPlaying = false;
        
            DateTime endTime = DateTime.Now;
            TimeSpan timeDif = endTime - startTime;
            curPlayTime = (float)timeDif.TotalSeconds;
            dataMgr.GameData.totalPlayTime += curPlayTime;
        }
        
        Application.Quit();
    }
}