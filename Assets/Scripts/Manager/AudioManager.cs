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
    
    private Dictionary<string, Sound> bgmDic = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> sfxDic = new Dictionary<string, Sound>();

    [HorizontalLine(color: EColor.Orange), BoxGroup("# Setting Audio Player"), SerializeField]
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
        
        UpdateAudioClip();
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

        //UpdateAudioPlayer();
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
        var players = GetComponentsInChildren<AudioSource>();

        sfxPlayerList.Clear();
        foreach(var player in players)
        {
            if (!player.name.Equals("BGMPlayer"))
                sfxPlayerList.Add(player);
        }
    }
    #endregion

    #region Update Audio Clip
    private void UpdateAudioClip()
    {
        bgmDic.Clear();
        sfxDic.Clear();
        
        string[] assetPaths =
        {
            "Audio/BGM",
            "Audio/SFX"
        };

        foreach (var path in assetPaths)
        {
            var clips = Resources.LoadAll<AudioClip>(path);

            foreach (var clip in clips)
            {
                int indexOfUnderscore = clip.name.LastIndexOf('_');
                if (indexOfUnderscore >= 0 && indexOfUnderscore < clip.name.Length - 1)
                {
                    string nameStr = clip.name.Substring(indexOfUnderscore + 1).Trim();
                    string idStr = Regex.Replace(clip.name, @"\D", "");

                    var targetDic = clip.name.Contains("BGM") ? bgmDic : sfxDic;
                    targetDic.Add(nameStr, new Sound(nameStr, int.Parse(idStr), clip));
                }
            }
        }

        bgmDic = bgmDic.OrderBy(obj => 
            obj.Value.id).ToDictionary(x => x.Key, x => x.Value);
        sfxDic = sfxDic.OrderBy(obj => 
            obj.Value.id).ToDictionary(x => x.Key, x => x.Value);
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
    public void PlayBGM(EBGMName bgmName)
    {
        if (bgmPlayer.clip != null && bgmPlayer.clip.name.Equals(bgmName.ToString()))
            return;

        if (bgmDic.TryGetValue(bgmName.ToString(), out Sound bgm))
        {
            bgmPlayer.clip = bgm.clip;
            bgmPlayer.Play();
        }
    }

    public void StopBGM() => bgmPlayer.Stop();

    public void PlaySFX(ESFXName sfxName)
    {
        if (sfxDic.TryGetValue(sfxName.ToString(), out Sound bgm))
        {
            for (int j = 0; j < sfxPlayerList.Count; j++)
            {
                if (!sfxPlayerList[j].isPlaying)
                {
                    sfxPlayerList[j].clip = bgm.clip;
                    sfxPlayerList[j].Play();
                    return;
                }
            }
        }
    }

    public void StopSFX(ESFXName sfxName)
    {
        if (sfxDic.TryGetValue(sfxName.ToString(), out Sound bgm))
        {
            var targetPlayer = sfxPlayerList.Find(x => x.clip == bgm.clip);
            if (targetPlayer)
            {
                targetPlayer.Stop();
                targetPlayer.clip = null;
            }
        }
    }
    #endregion
}