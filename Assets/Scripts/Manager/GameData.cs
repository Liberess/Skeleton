using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [Header("## Account Datas")]
    public EntityData playerData;
    public int level = 1;
    public float exp = 0.0f;
    public float needsExp = 5.0f;
    public int gold = 0;
    public int karma = 0;
    public readonly int MaxGoodsAmount = 999999999;

    [Header("## Game Datas"), Space(5)]
    public float totalPlayTime = 0.0f;
    public int killCount = 0;
    public int deathCount = 0;
    public int stageCount = 0;
    public string stageStr = "1-0";
    public float bgm = 50.0f;
    public float sfx = 50.0f;
    
    // 스텟 관련 정보 (업글)
    [Header("## Player Status Datas"), Space(5)]
    public int[] statUpLevels = new int[6];
    public readonly int MaxStatLevel = 999;
    
    [Header("## Skill Datas"), Space(5)]
    public int[] skillUpLevels = new int[3];
    public int[] skillEffectAmounts = new int[3];
    public readonly int MaxSkillLevel = 99;
    
    // 장비 관련 정보
    [Header("## Equipment Datas"), Space(5)]
    public List<EquipmentData> weaponDataList;
    public List<EquipmentData> armorDataList;
}