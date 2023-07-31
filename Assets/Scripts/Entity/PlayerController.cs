using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonMobileUI.Scripts;
using MonsterLove.StateMachine;
using NaughtyAttributes;

public class PlayerController : Entity
{
    private MonsterManager monsterMgr;

    public EStates state;

    [HorizontalLine(color: EColor.Red), BoxGroup("# Joystick Settings"), SerializeField]
    private StaticJoystickController staticJoystick;

    [BoxGroup("# Joystick Settings"), SerializeField]
    private DynamicJoystickController dynamicJoystick;

    private Vector2 moveInputVec;
    private Vector3 moveVec;

    private float curDist = 0.0f;
    private float closetDist = float.MaxValue;
    private float targetDist = float.MaxValue;
    private int closetIndex = 0;
    private int targetIndex = 0;
    
    [HorizontalLine(color: EColor.Yellow), BoxGroup("# Attack Effect Settings"), SerializeField]
    private TrailRenderer attackTrailRen;

    private bool isDashing = false;
    private bool isControlling = false;
    
    private static readonly int DoSkill = Animator.StringToHash("doSkill");

    protected override void Start()
    {
        monsterMgr = MonsterManager.Instance;

        EntityData = DataManager.Instance.GameData.playerData;
        fsm.ChangeState(EStates.Init);

        JoystickEventBinding();

        attackTrailRen.enabled = false;

        base.Start();
    }
    
    // Only Used in Attack Animation Event
    private void EnableAttackTrail() => attackTrailRen.enabled = true;
    
    // Only Used in Attack Animation Event
    private void DisableAttackTrail() => attackTrailRen.enabled = false;

    private void JoystickEventBinding()
    {
        dynamicJoystick.OnPointerDownAction += () =>
        {
            if (fsm.State != EStates.Skill && !isDashing)
            {
                isControlling = true;
                anim.SetBool(IsAttack, false);
                anim.SetTrigger("doDisabledAttack");
                fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
            }
        };
        
        dynamicJoystick.OnPointerUpAction += () =>
        {
            isControlling = false;
            if (HasTarget)
                fsm.ChangeState(EStates.Track);
            else
                fsm.ChangeState(EStates.Idle);
        };
        
        staticJoystick.OnPointerDownAction += () =>
        {
            if (fsm.State != EStates.Skill && !isDashing)
            {
                isControlling = true;
                anim.SetBool(IsAttack, false);
                anim.SetTrigger("doDisabledAttack");
                fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
            }
        };
        
        staticJoystick.OnPointerUpAction += () =>
        {
            isControlling = false;
            if (HasTarget)
                fsm.ChangeState(EStates.Track);
            else
                fsm.ChangeState(EStates.Idle);
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

    protected override void Control_FixedUpdate()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;

        moveVec = new Vector3(moveInputVec.x, 0f, moveInputVec.y) * EntityData.moveSpeed * Time.deltaTime;
        rigid.MovePosition(rigid.position + moveVec);

        if (moveVec.sqrMagnitude != 0)
        {
            Quaternion dirQuat = Quaternion.LookRotation(moveVec);
            Quaternion moveQuat = Quaternion.Slerp(rigid.rotation, dirQuat, 0.3f);
            rigid.MoveRotation(moveQuat);
        }
    }

    public void OnMoveInput(Vector2 moveInput) => moveInputVec = moveInput;
    
    protected override void Idle_Update()
    {
        // 플레이어의 경우에는 복수의 target이 존재하기에,
        // 매번 가장 가까운 target을 확인해야 한다.
        FindNearestMonster();
        
        if (HasTarget)
        {
            if(IsAttackable && IsAttached)
                fsm.ChangeState(EStates.Attack);
            else
                fsm.ChangeState(EStates.Track);
        }
    }

    /// <summary>
    /// 가장 가까이에 있는 target을 찾는다.
    /// </summary>
    private void FindNearestMonster()
    {
        if (monsterMgr.SpawnedMonsterList.Count > 0)
        {
            closetIndex = 0;
            targetIndex = -1;

            for (int i = 0; i < monsterMgr.SpawnedMonsterList.Count; i++)
            {
                Vector3 monsterPos = monsterMgr.SpawnedMonsterList[i].transform.position;
                curDist = Vector3.Distance(transform.position, monsterPos);

                if (Physics.Raycast(transform.position, monsterPos - transform.position,
                        20f, targetLayer.value))
                {
                    if (targetDist >= curDist)
                    {
                        targetIndex = i;
                        targetDist = curDist;
                    }

                    if (closetDist >= curDist)
                    {
                        closetIndex = i;
                        closetDist = curDist;
                    }
                }
            }

            if (targetIndex == -1)
                targetIndex = closetIndex;

            TargetEntity = monsterMgr.SpawnedMonsterList[targetIndex];

            closetDist = float.MaxValue;
            targetDist = float.MaxValue;
        }
    }

    protected override void Track_Update()
    {
        FindNearestMonster();
        
        if (HasTarget)
        {
            if(IsAttackable && IsAttached)
                fsm.ChangeState(EStates.Attack);

            if (!IsAttached && !isControlling)
            {
                var targetPosition = TargetEntity.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                    EntityData.moveSpeed * Time.deltaTime);
            }
            
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            
            if(!isControlling)
                RotateToTarget();
        }
        else
        {
            fsm.ChangeState(EStates.Idle);
        }
    }

    protected override void Attack_Update()
    {
        if (moveInputVec.sqrMagnitude != 0)
        {
            anim.SetBool(IsWalk, true);
            anim.SetBool(IsAttack, false);
            fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
            return;
        }

        if (HasTarget)
        {
            RotateToTarget();
                
            if (IsAttackable)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                
                if (!IsAttached)
                {
                    fsm.ChangeState(EStates.Track);
                    return;
                }
                    
                lastAttackTime = Time.time;
                anim.SetBool(IsAttack, true);
            }
        }
        else
        {
            TargetEntity = null;
            fsm.ChangeState(EStates.Idle);
        }
    }

    public override void OnAttack1Trigger()
    {
        AttackTargetEntity(EntityData.attackPower + EntityData.increaseAttackPower);
        fsm.ChangeState(EStates.Track);
    }

    protected override void Attack_Exit()
    {
        anim.SetBool(IsAttack, false);
        base.Attack_Exit();
    }

    protected override void Skill_Enter()
    {
        
    }

    protected override void Skill_Exit()
    {
        
    }

    public void UseSkill(ESkillType skillType)
    {
        fsm.ChangeState(EStates.Skill, StateTransition.Overwrite);
        
        int amount = DataManager.Instance.GameData.skillEffectAmounts[(int)skillType];
        int damageAmount = (EntityData.attackPower + EntityData.increaseAttackPower) * (100 + amount) / 100;
        switch (skillType)
        {
            case ESkillType.PhantomBlade:
                AudioManager.Instance.PlaySFX(ESFXName.Blade);
                if (IsAttached)
                {
                    anim.SetTrigger(DoSkill);
                    AttackTargetEntity(damageAmount);
                }
                else
                {
                    StartCoroutine(DashCo(damageAmount));
                }
                break;
            
            case ESkillType.FireBall:
                anim.SetTrigger(DoSkill);
                AudioManager.Instance.PlaySFX(ESFXName.FireBall);
                SkillSO skillSo = DataManager.Instance.GetSkillSO(skillType);
                DamageMessage dmgMsg = new DamageMessage(this.gameObject, damageAmount);
                
                Vector3 fireBallSpawnOffset = Vector3.up;
                
                var fireBall = EffectManager.Instance.InstantiateObj(EEffectType.FireBall);
                fireBall.transform.position = transform.position + fireBallSpawnOffset;
                
                // FireBall의 위치에서 Target의 위치까지의 방향 계산
                Vector3 targetPosition = new Vector3(TargetEntity.transform.position.x, transform.position.y, TargetEntity.transform.position.z);
                Vector3 fireBallDirection = (targetPosition - fireBall.transform.position).normalized;
                
                // FireBall이 계산된 방향을 보도록 설정
                fireBall.transform.rotation = Quaternion.LookRotation(fireBallDirection);
                
                fireBall.GetComponent<Projectile>().SetupProjectile(dmgMsg, targetLayer, skillSo.projectileVelocity,
                    skillSo.projectileDistance, skillSo.skillImpactRange);
                break;
            
            case ESkillType.Recovery:
                AudioManager.Instance.PlaySFX(ESFXName.Recovery);
                fsm.ChangeState(EStates.Attack);
                RecoveryHealthPoint(amount);
                break;
        }
    }

    public void OnDisableUseSkill()
    {
        fsm.ChangeState(EStates.Attack);
    }

    private IEnumerator DashCo(int amount)
    {
        isDashing = true;
        
        while (true)
        {
            yield return null;

            if (!IsAttached)
            {
                var targetPosition = TargetEntity.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                    EntityData.moveSpeed * 2.0f * Time.deltaTime);
            }
            else
            {
                break;
            }
        }
        
        anim.SetTrigger(DoSkill);
        AttackTargetEntity(amount);
        isDashing = false;
    }

    public override void ApplyDamage(DamageMessage dmgMsg)
    {
        AudioManager.Instance.PlaySFX(ESFXName.PlayerHit);
        base.ApplyDamage(dmgMsg);
    }

    protected override void Die_Enter()
    {
        AudioManager.Instance.PlaySFX(ESFXName.PlayerDie);
        GameManager.Instance.GameOverAction?.Invoke();
        base.Die_Enter();
    }

    public void ControlJoystick(EJoystickType type, bool active)
    {
        dynamicJoystick.gameObject.SetActive(type == EJoystickType.Dynamic && active);
        staticJoystick.transform.parent.gameObject.SetActive(type == EJoystickType.Static && active);
    }
}