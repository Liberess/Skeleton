using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MonsterLove.StateMachine;

public class Monster : Entity
{
    private NavMeshAgent agent;

    public EStates state;
    private static readonly int DoAttack = Animator.StringToHash("doAttack");

    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        base.Awake();
    }

    protected override void OnEnable()
    {
        targetEntity = FindObjectOfType<PlayerController>();

        agent.enabled = true;
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        
        DeathAction += () =>
        {
            agent.isStopped = true;
            agent.enabled = false;
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

    public override void SetupEntityData(EntityData entityData, float increaseValue = 1.0f)
    {
        base.SetupEntityData(entityData, increaseValue);

        agent.isStopped = false;
        agent.stoppingDistance = entityData.attackRange * 0.5f;
        agent.speed = entityData.moveSpeed;
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
}
