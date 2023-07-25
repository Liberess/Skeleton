using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MonsterLove.StateMachine;

public class Monster : Entity
{
    private NavMeshAgent agent;
    
    private static readonly int DoAttack = Animator.StringToHash("doAttack");
    
    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        base.Awake();
    }

    protected override void OnEnable()
    {
        targetEntity = FindObjectOfType<PlayerController>();
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        
        DeathAction += () =>
        {
            agent.isStopped = true;
            agent.enabled = false;
            //anim.SetTrigger("doDie");
            gameObject.layer = LayerMask.NameToLayer("Ignore");
        };
        
        //MonsterManager.Instance.ReturnObj(this, 2.0f);
    }

    private void Update()
    {
        state = fsm.State;
        fsm.Driver.Update?.Invoke();
    }

    private void FixedUpdate()
    {
        fsm.Driver.FixedUpdate?.Invoke();
    }

    protected override void Init_Enter()
    {
        agent.isStopped = false;
        agent.stoppingDistance = entitySO.AttackRange * 0.5f;
        agent.speed = entitySO.MoveSpeed;
        
        base.Init_Enter();
    }

    protected override void TrackFlow()
    {
        if (!agent.pathPending)
            agent.SetDestination(targetEntity.transform.position);
    }

    protected override void Track_Enter()
    {
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        base.Track_Enter();
    }

    protected override void Track_Exit()
    {
        agent.velocity = Vector3.zero;
        base.Track_Exit();
    }

    protected override void AttackFlow()
    {
        if (!IsDead)
        {
            if (HasTarget && IsAttackable)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.updatePosition = false;
                agent.updateRotation = false;
                
                lastAttackTime = Time.time;

                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    anim.SetTrigger(DoAttack);
            }
        }
    }

    protected override void Die_Enter()
    {
        agent.isStopped = true;
        base.Die_Enter();
    }
}
