using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using MonsterLove.StateMachine;
using UnityEngine.UI;

public abstract class Entity : LivingEntity
{
    public enum EStates
    {
        Init,
        Idle,
        Control,
        Track,
        Attack,
        Die
    }

    protected StateMachine<EStates> fsm;

    [SerializeField] private EntityData entityData;

    public EntityData EntityData
    {
        get { return entityData; }
        protected set { entityData = value; }
    }

    // Attack
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LivingEntity targetEntity;
    protected bool HasTarget => targetEntity != null && !targetEntity.IsDead;

    protected float lastAttackTime = 0.0f;
    protected bool IsAttackable => Time.time >= lastAttackTime + EntityData.attackPerSecond;

    [SerializeField] private Slider hpBar;

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
            attackRange = Mathf.RoundToInt(
                Mathf.Clamp(entityData.attackRange * increaseValue, 1.0f, entityData.maxAttackRange)),
            attackPerSecond = Mathf.RoundToInt(
                Mathf.Clamp(entityData.attackPerSecond * increaseValue, 0.1f, entityData.maxAttackPerSecond)),
            moveSpeed = Mathf.RoundToInt(
                Mathf.Clamp(entityData.moveSpeed * increaseValue, 0.5f, entityData.maxMoveSpeed))
        };

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
        hpBar.maxValue = 1.0f;
        hpBar.value = (float)CurrentHp / EntityData.healthPoint;
    }

    protected virtual void Init_Enter()
    {
        CurrentHp = EntityData.healthPoint;
        
        UpdateHpUI();

        if (targetEntity == null)
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
        if (targetEntity == null || targetEntity && !targetEntity.gameObject.activeSelf)
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
                    targetEntity = entity;
                    fsm.ChangeState(EStates.Track);
                }
            }
            else
            {
                targetEntity = null;
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
        if (targetEntity != null)
        {
            float distance = Vector3.Distance(targetEntity.transform.position, transform.position);
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
        
    }

    protected virtual void Attack_Update() => AttackFlow();
    protected abstract void AttackFlow();

    protected void OnAttack1Trigger()
    {
        DamageMessage dmgMsg = new DamageMessage(this.gameObject, EntityData.attackPower);
        targetEntity.ApplyDamage(dmgMsg);

        fsm.ChangeState(EStates.Track);
    }

    protected virtual void Attack_Exit()
    {
        
    }

    protected virtual void Die_Enter()
    {
        StopAllCoroutines();
    }
}