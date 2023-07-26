using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int gold = 0;
    public int karma = 0;

    public float totalPlayTime = 0.0f;

    public int killCount = 0;
    public int deathCount = 0;
    public int stageCount = 0;

    public string stageStr = "1-0";

    public float bgm = 50.0f;
    public float sfx = 50.0f;

    public EntityData playerData;

    // 스텟 관련 정보 (업글)
    public int[] statUpLevels = new int[3];
}