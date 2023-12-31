using UnityEngine;

[System.Serializable]
public class EntityData
{
    public string entityName = "None";
    public EEntityType entityType;

    public int healthPoint = 10;

    public int attackPower = 1;
    [Range(1.0f, 100.0f)] public float attackRange = 1.0f;
    [Range(1.0f, 100.0f)] public float maxAttackRange = 2.0f;
    [Range(0.1f, 100.0f)] public float attackPerSecond = 1.0f;
    [Range(0.1f, 100.0f)] public float maxAttackPerSecond = 2.0f;
    [Range(0.1f, 100.0f)] public float moveSpeed = 3.0f;
    [Range(0.5f, 100.0f)] public float maxMoveSpeed = 5.0f;

    [ReadOnly] public int increaseHealthPoint = 0;
    [ReadOnly] public int increaseAttackPower = 0;
    
    public float DPS => // 초당 공격력. Damage per Second
        (1f / attackPerSecond) * attackPower;

    public EntityData(EntityData entityData)
    {
        entityName = entityData.entityName;
        healthPoint = entityData.healthPoint;
        attackPower = entityData.attackPower;
        attackRange = entityData.attackRange;
        attackPerSecond = entityData.attackPerSecond;
        moveSpeed = entityData.moveSpeed;

        maxAttackRange = entityData.maxAttackRange;
        maxAttackPerSecond = entityData.maxAttackPerSecond;
        maxMoveSpeed = entityData.maxMoveSpeed;

        increaseHealthPoint = entityData.increaseHealthPoint;
        increaseAttackPower = entityData.increaseAttackPower;
    }
}
