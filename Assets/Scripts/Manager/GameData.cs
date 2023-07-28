using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [Header("## Account Datas")]
    [ReadOnly] public EntityData playerData;
    [ReadOnly] public int level = 1;
    [ReadOnly] public float exp = 0.0f;
    [ReadOnly] public float needsExp = 5.0f;
    [ReadOnly] public int gold = 0;
    [ReadOnly] public int karma = 0;
    public readonly int MaxGoodsAmount = 999999999;

    [Header("## Game Datas"), Space(5)]
    [ReadOnly] public float totalPlayTime = 0.0f;
    [ReadOnly] public int killCount = 0;
    [ReadOnly] public int deathCount = 0;
    [ReadOnly] public int stageCount = 0;
    [ReadOnly] public string stageStr = "1-0";
    [ReadOnly] public float bgm = 50.0f;
    [ReadOnly] public float sfx = 50.0f;
    
    // 스텟 관련 정보 (업글)
    [Header("## Player Status Datas"), Space(5)]
    [ReadOnly] public int[] statUpLevels = new int[6];
    public readonly int MaxStatLevel = 999;
    
    [Header("## Skill Datas"), Space(5)]
    [ReadOnly] public int[] skillUpLevels = new int[3];
    [ReadOnly] public int[] skillEffectAmounts = new int[3];
    public readonly int MaxSkillLevel = 99;
    
    // 장비 관련 정보
    [Header("## Equipment Datas"), Space(5)]
    [ReadOnly] public List<EquipmentData> weaponDataList;
    [ReadOnly] public List<EquipmentData> armorDataList;
}