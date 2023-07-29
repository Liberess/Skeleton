using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Monster : Entity
{
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

    protected override void TrackFlow()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetEntity.transform.position,
            EntityData.moveSpeed * Time.deltaTime);

        RotateToTarget();
    }

    protected override void Attack_Update()
    {
        if (!IsDead)
        {
            if (HasTarget)
            {
                Debug.Log("hasTarget");
                RotateToTarget();
                
                if (IsAttackable)
                {
                    if (!IsAttached)
                    {
                        Debug.Log("멀어서 새로 추적");
                        fsm.ChangeState(EStates.Track);
                        return;
                    }
                    
                    Debug.Log("attack");
                    lastAttackTime = Time.time;

                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                        anim.SetBool(IsAttack, true);
                }
            }
            else
            {
                Debug.Log("target null, go idle");
                TargetEntity = null;
                fsm.ChangeState(EStates.Idle);
            }
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
