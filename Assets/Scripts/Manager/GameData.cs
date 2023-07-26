using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int level = 1;
    public float exp = 0.0f;
    public float needsExp = 5.0f;
    public int gold = 0;
    public int karma = 0;
    public readonly int MaxGoodsAmount = 999999999;

    public float totalPlayTime = 0.0f;

    public int killCount = 0;
    public int deathCount = 0;
    public int stageCount = 0;

    public string stageStr = "1-0";

    public float bgm = 50.0f;
    public float sfx = 50.0f;

    public EntityData playerData;

    // 스텟 관련 정보 (업글)
    public int[] statUpLevels = new int[6];
    public int[] skillUpLevels = new int[3];
    public float[] skillEffectAmounts = new float[3];
    public readonly int MaxStatLevel = 999;
    public readonly int MaxSkillLevel = 99;
}