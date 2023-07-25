using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;

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

    [SerializeField] protected EntitySO entityData; 

    // Attack
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LivingEntity targetEntity;
    protected bool HasTarget => targetEntity != null && !targetEntity.IsDead;

    protected float lastAttackTime = 0.0f;
    protected bool IsAttackable => Time.time >= lastAttackTime + entityData.AttackPerSecond;

    [SerializeField] protected EStates state;

    protected Animator anim;
    protected Rigidbody rigid;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();

        fsm = new StateMachine<EStates>(this);
        fsm.ChangeState(EStates.Init);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        fsm.ChangeState(EStates.Init);
    }

    protected virtual void Start()
    {
        DeathAction += () =>
        {
            //anim.SetTrigger("doDie");
            gameObject.layer = LayerMask.NameToLayer("Ignore");
        };
    }
    
    protected virtual void Init_Enter()
    {
        Debug.Log("Init_Enter");
        if(targetEntity == null)
            fsm.ChangeState(EStates.Idle);
        else
            fsm.ChangeState(EStates.Track);
    }
    
    protected virtual void Idle_Enter()
    {
        Debug.Log("Idle_Enter");
    }
    
    protected virtual void Idle_Update()
    {
        Debug.Log("Idle_Update");
        if (targetEntity == null || targetEntity && !targetEntity.gameObject.activeSelf)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, 50f, targetLayer);
            if (cols.Length > 0)
            {
                foreach (var col in cols)
                {
                    if (col.TryGetComponent(out LivingEntity entity))
                    {
                        targetEntity = entity;
                        fsm.ChangeState(EStates.Track);
                    }
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
        Debug.Log("Idle_Exit");
    }

    protected virtual void Control_Enter()
    {
        Debug.Log("Control_Enter");
    }

    protected virtual void Control_FixedUpdate()
    {
        Debug.Log("Control_FixedUpdate");
    }

    protected virtual void Control_Exit()
    {
        Debug.Log("Control_Exit");
    }

    protected virtual void Track_Enter()
    {
        //anim.SetBool("isWalk", true);
        Debug.Log(name + " :: Track_Enter");
    }

    protected virtual void Track_Update()
    {
        Debug.Log(name + " :: Track_Update");
        float distance = Vector3.Distance(targetEntity.transform.position, transform.position);
        if (distance <= entityData.AttackRange && fsm.State != EStates.Attack)
            fsm.ChangeState(EStates.Attack);
        else
            TrackFlow();
    }

    protected abstract void TrackFlow();

    protected virtual void Track_Exit()
    {
        //anim.SetBool("isWalk", false);
    }
    
    protected virtual void Attack_Enter()
    {

    }

    protected virtual void Attack_Update() => AttackFlow();
    protected abstract void AttackFlow();

    protected virtual void Attack_Exit()
    {
        
    }

    public override void Die()
    {
        fsm.ChangeState(EStates.Die);
        base.Die();
    }

    protected virtual void Die_Enter()
    {
        Debug.Log(name + " :: Die_Enter");
        StopAllCoroutines();
    }
}
