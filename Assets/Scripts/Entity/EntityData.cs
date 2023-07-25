using UnityEngine;

[System.Serializable]
public class EntityData
{
    public string entityName = "None";

    public int healthPoint = 10;

    public int attackPower = 1;
    [Range(0.1f, 100.0f)] public float attackRange = 1.0f;
    [Range(0.1f, 100.0f)] public float attackPerSecond = 1.0f;
    [Range(0.1f, 100.0f)] public float moveSpeed = 3.0f;
}
