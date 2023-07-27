using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonsterLove.StateMachine;
using NaughtyAttributes;

public abstract class Entity : LivingEntity
{
    public enum EStates
    {
        Init,
        Idle,
        Control,
        Track,
        Attack,
        Skill,
        Die
    }

    protected StateMachine<EStates> fsm;

    [Foldout("# Status Settings")]
    [SerializeField] private EntityData entityData;

    public EntityData EntityData
    {
        get { return entityData; }
        protected set { entityData = value; }
    }

    [Foldout("# Status Settings")]
    [SerializeField] private Slider hpBar;
    
    // Attack
    [Foldout("# Attack Settings")]
    [SerializeField] protected LayerMask targetLayer;

    [SerializeField] private LivingEntity targetEntity;
    public LivingEntity TargetEntity
    {
        get { return targetEntity; }
        protected set { targetEntity = value; }
    }

    protected bool HasTarget => TargetEntity != null && !TargetEntity.IsDead;

    protected float lastAttackTime = 0.0f;

    protected bool IsAttackable =>
        Time.time >= lastAttackTime + EntityData.attackPerSecond;

    public bool IsAttached =>
        TargetEntity != null && Vector3.Distance(TargetEntity.transform.position, transform.position) <= entityData.attackRange;
    
    protected Animator anim;
    protected Rigidbody rigid;

    protected static readonly int IsWalk = Animator.StringToHash("isWalk");
    protected static readonly int DoDie = Animator.StringToHash("doDie");

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();

        fsm = new StateMachine<EStates>(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EntityData = null;
    }

    public virtual void SetupEntityData(EntityData entityData, float increaseValue = 1.0f)
    {
        EntityData = new EntityData(entityData)
        {
            healthPoint = Mathf.RoundToInt(entityData.healthPoint * increaseValue),
            attackPower = Mathf.RoundToInt(entityData.attackPower * increaseValue),
            attackPerSecond = Mathf.RoundToInt(
                Mathf.Clamp(entityData.attackPerSecond * increaseValue, 0.1f, entityData.maxAttackPerSecond)),
            moveSpeed = Mathf.RoundToInt(
                Mathf.Clamp(entityData.moveSpeed * increaseValue, 0.5f, entityData.maxMoveSpeed))
        };

        originHp = this.entityData.healthPoint;

        fsm.ChangeState(EStates.Init);
    }

    protected virtual void Start()
    {
        DeathAction += () =>
        {
            fsm.ChangeState(EStates.Die);
            anim.SetTrigger(DoDie);
            gameObject.layer = LayerMask.NameToLayer("Ignore");
        };

        ChangedHpValueAction += () => hpBar.value = (float)CurrentHp / EntityData.healthPoint;
    }

    public void UpdateHpUI()
    {
        originHp = EntityData.healthPoint;
        
        hpBar.maxValue = 1.0f;
        hpBar.value = (float)CurrentHp / EntityData.healthPoint;
    }

    protected virtual void Init_Enter()
    {
        originHp = EntityData.healthPoint;
        CurrentHp = originHp;
        
        UpdateHpUI();

        if (TargetEntity == null)
            fsm.ChangeState(EStates.Idle);
        else
            fsm.ChangeState(EStates.Track);
    }

    protected virtual void Idle_Enter()
    {
        anim.SetBool(IsWalk, false);
    }

    protected virtual void Idle_Update()
    {
        if (TargetEntity == null || TargetEntity && !TargetEntity.gameObject.activeSelf)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, 100f, targetLayer);
            if (cols.Length > 0)
            {
                List<GameObject> objList = new List<GameObject>();
                foreach (var col in cols)
                    objList.Add(col.gameObject);

                GameObject nearstObj = Utility.GetNearestObjectByList(objList, transform.position);
                if (nearstObj.TryGetComponent(out LivingEntity entity))
                {
                    TargetEntity = entity;
                    fsm.ChangeState(EStates.Track);
                }
            }
            else
            {
                TargetEntity = null;
            }
        }
        else
        {
            fsm.ChangeState(EStates.Track);
        }
    }

    protected virtual void Idle_Exit()
    {
    }

    protected virtual void Control_Enter()
    {
        anim.SetBool(IsWalk, true);
    }

    protected virtual void Control_FixedUpdate()
    {
    }

    protected virtual void Control_Exit()
    {
        anim.SetBool(IsWalk, false);
    }

    protected virtual void Track_Enter()
    {
        anim.SetBool(IsWalk, true);
    }

    protected virtual void Track_Update()
    {
        if (TargetEntity != null)
        {
            float distance = Vector3.Distance(TargetEntity.transform.position, transform.position);
            if (distance <= EntityData.attackRange && fsm.State != EStates.Attack)
                fsm.ChangeState(EStates.Attack);
            else
                TrackFlow();
        }
    }

    protected abstract void TrackFlow();

    protected virtual void Track_Exit()
    {
        anim.SetBool(IsWalk, false);
    }

    protected virtual void Attack_Enter()
    {
        if(!HasTarget)
            fsm.ChangeState(EStates.Idle);
    }

    protected virtual void Attack_Update() => AttackFlow();
    protected abstract void AttackFlow();

    protected void OnAttack1Trigger()
    {
        AttackTargetEntity();
        fsm.ChangeState(EStates.Track);
    }

    protected void AttackTargetEntity(int damage = 0)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, TargetEntity.transform.position - transform.position, out hit,
                EntityData.attackRange, targetLayer))
        {
            if (hit.collider.gameObject != TargetEntity.gameObject)
            {
                if (hit.collider.TryGetComponent(out LivingEntity otherEntity))
                    TargetEntity = otherEntity;
            }

            DamageMessage dmgMsg = new DamageMessage(this.gameObject, damage > 0 ? damage : EntityData.attackPower, hit.point);
            TargetEntity.ApplyDamage(dmgMsg);
        }
    }

    protected virtual void Attack_Exit()
    {
        
    }

    protected virtual void Skill_Enter()
    {
        
    }

    protected virtual void Skill_Exit()
    {
        
    }

    protected virtual void Die_Enter()
    {
        StopAllCoroutines();
    }
}