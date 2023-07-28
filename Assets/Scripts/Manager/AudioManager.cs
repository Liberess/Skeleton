using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;
using NaughtyAttributes;
using TMPro;

[System.Serializable]
public struct Sound
{
    public string name;
    public AudioClip clip;

    public Sound(string _name, AudioClip _clip)
    {
        name = _name;
        clip = _clip;
    }
}

public enum EBGMName
{
    Main,
    InGame
}

public enum ESFXName
{
    GunFire,
    Explosion,
    PlayerHit,
    PlayerDie,
    ZombieHit,
    ZombieDie,
    UIClick,
    Reload
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
    private List<Sound> bgm = new List<Sound>();
    [BoxGroup("# Setting Audio Clip"), SerializeField]
    private List<Sound> sfx = new List<Sound>();
    
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
        UpdateBGMResource();
        UpdateSFXResource();
    }

    private void UpdateBGMResource()
    {
        bgm.Clear();

        AudioClip[] srcs = Resources.LoadAll<AudioClip>("Audio/BGM");

        foreach (var src in srcs)
            bgm.Add(new Sound(src.name.Substring(6), src));
    }

    private void UpdateSFXResource()
    {
        sfx.Clear();

        AudioClip[] srcs = Resources.LoadAll<AudioClip>("Audio/SFX");

        foreach (var src in srcs)
            sfx.Add(new Sound(src.name.Substring(6), src));
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

        for (int i = 0; i < bgm.Count; i++)
        {
            if (bgmName.Equals(bgm[i].name))
            {
                bgmPlayer.clip = bgm[i].clip;
                bgmPlayer.Play();
            }
        }
    }

    public void StopBGM() => bgmPlayer.Stop();

    public void PlaySFX(ESFXName _name)
    {
        string sfxName = _name.ToString();

        for (int i = 0; i < sfx.Count; i++)
        {
            if (sfxName.Equals(sfx[i].name))
            {
                for (int x = 0; x < sfxPlayerList.Count; x++)
                {
                    if (!sfxPlayerList[x].isPlaying)
                    {
                        sfxPlayerList[x].clip = sfx[i].clip;
                        sfxPlayerList[x].Play();
                        return;
                    }
                    else
                    {
                        if (sfxPlayerList[x].clip == sfx[i].clip)
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

        for (int i = 0; i < sfx.Count; i++)
        {
            if (sfxName.Equals(sfx[i].name))
            {
                for (int x = 0; x < sfxPlayerList.Count; x++)
                {
                    if (sfxPlayerList[x].isPlaying && sfxPlayerList[x].clip == sfx[i].clip)
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

    public AudioClip GetBGMClip(EBGMName bgmName) => bgm[(int)bgmName].clip;
    public AudioClip GetSFXClip(ESFXName sfxName) => bgm[(int)sfxName].clip;
}