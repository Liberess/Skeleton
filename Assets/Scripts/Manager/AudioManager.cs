using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using NaughtyAttributes;
using TMPro;

[System.Serializable]
public struct Sound
{
    public string name;
    public int id;
    public AudioClip clip;

    public Sound(string _name, int _id, AudioClip _clip)
    {
        name = _name;
        id = _id;
        clip = _clip;
    }
}

public enum EBGMName
{
    InGame
}

public enum ESFXName
{
    Click,
    PlayerHit,
    PlayerAttack,
    PlayerDie,
    Blade,
    FireBall,
    Explosion,
    Recovery,
    LevelUp,
    MonsterHit,
    MonsterAttack,
    MonsterDie,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [HorizontalLine(color: EColor.Red), BoxGroup("# Setting Audio Controller"), SerializeField]
    private AudioMixer masterMixer;
    [BoxGroup("# Setting Audio Controller"), SerializeField]
    private Slider bgmSlider;
    [BoxGroup("# Setting Audio Controller"), SerializeField]
    private Slider sfxSlider;

    [HorizontalLine(color: EColor.Orange), BoxGroup("# Setting Audio Clip"), SerializeField]
    private List<Sound> bgmList = new List<Sound>();
    [BoxGroup("# Setting Audio Clip"), SerializeField]
    private List<Sound> sfxList = new List<Sound>();
    
    [HorizontalLine(color: EColor.Yellow), BoxGroup("# Setting Audio Player"), SerializeField]
    private AudioSource bgmPlayer = null;
    [BoxGroup("# Setting Audio Player"), SerializeField]
    private List<AudioSource> sfxPlayerList = new List<AudioSource>();

    [BoxGroup("# Setting Audio Player")]
    public List<AudioSource> otherSfxPlayerList = new List<AudioSource>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        InitializedAudio();
    }
    
    public void InitializedAudio()
    {
        bgmSlider.maxValue = 100f;
        sfxSlider.maxValue = 100f;

        bgmSlider.value = DataManager.Instance.GameData.bgm;
        sfxSlider.value = DataManager.Instance.GameData.sfx;
        
        masterMixer.SetFloat("BGM", bgmSlider.value / 100f);
        masterMixer.SetFloat("SFX", sfxSlider.value / 100f);
    }

    #region UpdateAudioPlayer
    [ContextMenu("Update Audio Player Source")]
    private void UpdateAudioPlayer()
    {
        UpdateBGMPlayer();
        UpdateSFXPlayer();
    }

    private void UpdateBGMPlayer()
    {
        var child = transform.GetChild(0);
        bgmPlayer = child.GetComponent<AudioSource>();
    }

    private void UpdateSFXPlayer()
    {
        var players = FindObjectsOfType<AudioSource>();

        sfxPlayerList.Clear();
        foreach(var player in players)
        {
            if (player.name != "BGMPlayer")
                sfxPlayerList.Add(player);
        }
    }
    #endregion

    #region Update Audio Clip
    [ContextMenu("Update Audio Clip Resource")]
    private void UpdateAudioClip()
    {
        bgmList.Clear();
        sfxList.Clear();
        
        string[] assetPaths =
        {
            "Assets/Audios/BGM",
            "Assets/Audios/SFX"
        };

        var guids = AssetDatabase.FindAssets("t:AudioClip", assetPaths);
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip data = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (data != null)
            {
                int indexOfUnderscore = data.name.LastIndexOf('_');
                if (indexOfUnderscore >= 0 && indexOfUnderscore < data.name.Length - 1)
                {
                    string nameStr = data.name.Substring(indexOfUnderscore + 1).Trim();
                    string idStr = Regex.Replace(data.name, @"\D", "");
                    List<Sound> targetList = data.name.Contains("BGM") ? bgmList : sfxList;
                    targetList.Add(new Sound(nameStr, int.Parse(idStr), data));
                }
            }
        }

        bgmList = bgmList.OrderBy(obj => obj.id).ToList();
        sfxList = sfxList.OrderBy(obj => obj.id).ToList();
    }
        
    #endregion

    #region Audio Save
    public void BGMSave()
    {
        float targetSliderValue = bgmSlider.value / 100f;
        bgmPlayer.volume = targetSliderValue;
        bgmSlider.GetComponentInChildren<TextMeshProUGUI>().text =  $"{targetSliderValue:P0}";
        DataManager.Instance.GameData.bgm = bgmSlider.value;
    }

    public void SFXSave()
    {
        float targetSliderValue = sfxSlider.value / 100f;
        for (int i = 0; i < sfxPlayerList.Count; i++)
            sfxPlayerList[i].volume = targetSliderValue;
        foreach (var srcPlayer in otherSfxPlayerList)
            srcPlayer.volume = sfxSlider.value / 100f;
        
        sfxSlider.GetComponentInChildren<TextMeshProUGUI>().text =  $"{targetSliderValue:P0}";
        DataManager.Instance.GameData.sfx = sfxSlider.value;
    }
    #endregion

    #region Audio Play & Stop
    public void PlayBGM(EBGMName _name)
    {
        var bgmName = _name.ToString();

        if (bgmPlayer.clip != null && bgmPlayer.clip.name == bgmName)
            return;

        for (int i = 0; i < bgmList.Count; i++)
        {
            if (bgmName.Equals(bgmList[i].name))
            {
                bgmPlayer.clip = bgmList[i].clip;
                bgmPlayer.Play();
            }
        }
    }

    public void StopBGM() => bgmPlayer.Stop();

    public void PlaySFX(ESFXName _name)
    {
        string sfxName = _name.ToString();

        for (int i = 0; i < sfxList.Count; i++)
        {
            if (sfxName.Equals(sfxList[i].name))
            {
                for (int x = 0; x < sfxPlayerList.Count; x++)
                {
                    if (!sfxPlayerList[x].isPlaying)
                    {
                        sfxPlayerList[x].clip = sfxList[i].clip;
                        sfxPlayerList[x].Play();
                        return;
                    }
                    else
                    {
                        if (sfxPlayerList[x].clip == sfxList[i].clip)
                            return;
                    }
                }
                return;
            }
        }
    }

    public void StopSFX(ESFXName _name)
    {
        var sfxName = _name.ToString();

        for (int i = 0; i < sfxList.Count; i++)
        {
            if (sfxName.Equals(sfxList[i].name))
            {
                for (int x = 0; x < sfxPlayerList.Count; x++)
                {
                    if (sfxPlayerList[x].isPlaying && sfxPlayerList[x].clip == sfxList[i].clip)
                    {
                        sfxPlayerList[x].Stop();
                        sfxPlayerList[x].clip = null;
                    }
                }
                return;
            }
        }
    }
    #endregion

    public AudioClip GetBGMClip(EBGMName bgmName) => bgmList[(int)bgmName].clip;
    public AudioClip GetSFXClip(ESFXName sfxName) => bgmList[(int)sfxName].clip;
}