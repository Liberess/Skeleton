using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Entity
{
    public EMonsterType MonsterType { get; private set; }
    
    public EStates state;

    protected override void OnEnable()
    {
        TargetEntity = FindObjectOfType<PlayerController>();
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        
        DeathAction += () =>
        {
            int min = DataManager.Instance.GameData.stageCount;
            GameManager.Instance.SetGold(Mathf.RoundToInt(Random.Range(min, min * 2.5f)));
            GameManager.Instance.SetKarma(Mathf.RoundToInt(Random.Range(min, min * 2.5f)));
            GameManager.Instance.Exp += Random.Range(min, min * 2.5f);
            gameObject.layer = LayerMask.NameToLayer("Ignore");
        };
    }

    private void Update()
    {
        fsm.Driver.Update?.Invoke();
        state = fsm.State;
    }

    private void FixedUpdate()
    {
        fsm.Driver.FixedUpdate?.Invoke();
    }
    
    /// <summary>
    /// 몬스터의 데이터를 세팅한다.
    /// </summary>
    /// <param name="entityData">몬스터의 기본 데이터</param>
    /// <param name="increaseValue">증가될 수치</param>
    public virtual void SetupEntityData(EMonsterType monsterType, EntityData entityData, float increaseValue)
    {
        MonsterType = monsterType;
        
        EntityData = new EntityData(entityData)
        {
            entityName = entityData.entityName,
            entityType = entityData.entityType,
            healthPoint = Mathf.RoundToInt(entityData.healthPoint + increaseValue),
            attackPower = Mathf.RoundToInt(entityData.attackPower + increaseValue),
            attackRange = entityData.attackRange,
            maxAttackRange = entityData.maxAttackRange,
            attackSpeed = Mathf.Clamp(entityData.attackSpeed + 0.1f, entityData.attackSpeed, entityData.maxAttackSecond),
            maxAttackSecond = entityData.maxAttackSecond,
            moveSpeed = Mathf.Clamp(entityData.moveSpeed + 0.1f, entityData.moveSpeed, entityData.maxMoveSpeed),
            maxMoveSpeed = entityData.maxMoveSpeed,
            increaseHealthPoint = entityData.increaseHealthPoint,
            increaseAttackPower = entityData.increaseAttackPower
        };

        originHp = EntityData.healthPoint;
        fsm.ChangeState(EStates.Init);
    }

    protected override void Track_Update()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;
        
        if (HasTarget)
        {
            if (IsAttackable && IsAttached)
                fsm.ChangeState(EStates.Attack);

            if (!IsAttached)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetEntity.transform.position,
                    EntityData.moveSpeed * Time.deltaTime);
            }
            
            RotateToTarget();
            
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    protected override void Attack_Update()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;
        
        if (HasTarget)
        {
            RotateToTarget();
                
            if (IsAttackable)
            {
                if (!IsAttached)
                {
                    fsm.ChangeState(EStates.Track);
                    return;
                }

                lastAttackTime = Time.time;
                
                if(anim.GetBool(IsAttack))
                    anim.SetBool(IsAttack, false);
                anim.SetBool(IsAttack, true);
                    
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            TargetEntity = null;
            fsm.ChangeState(EStates.Idle);
        }
    }

    public override void ApplyDamage(DamageMessage dmgMsg)
    {
        AudioManager.Instance.PlaySFX(ESFXName.MonsterHit);
        base.ApplyDamage(dmgMsg);
    }

    protected override void Die_Enter()
    {
        AudioManager.Instance.PlaySFX(ESFXName.MonsterDie);
        base.Die_Enter();
    }
}
