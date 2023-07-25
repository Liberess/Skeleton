using UnityEngine;

[CreateAssetMenu(fileName = "Entity Data", menuName = "Scriptable Object/Entity Data", order = int.MaxValue)]
public class EntitySO : ScriptableObject
{
    [SerializeField] private string entityName = "None";
    public string EntityName => entityName;

    [SerializeField] private int healthPoint = 10;
    public int HealthPoint => healthPoint;

    [SerializeField] private int attackPower = 1; //공격력
    public int AttackPower => attackPower;

    [SerializeField, Range(0.1f, 100.0f)] private float attackRange = 1.0f; //공격 사거리
    public float AttackRange => attackRange;

    [SerializeField, Range(0.1f, 100.0f)] private float attackPerSecond = 1.0f; //초당 공격 횟수
    public float AttackPerSecond => attackPerSecond;

    [SerializeField, Range(0.1f, 100.0f)] private float moveSpeed = 3.0f;
    public float MoveSpeed => moveSpeed;
}
