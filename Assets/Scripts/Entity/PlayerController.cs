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

    [Foldout("# Joystick Settings"), SerializeField]
    private StaticJoystickController staticJoystick;

    [Foldout("# Joystick Settings"), SerializeField]
    private DynamicJoystickController dynamicJoystick;

    private Vector2 moveInputVec;
    private Vector3 moveVec;

    private float curDist = 0.0f;
    private float closetDist = float.MaxValue;
    private float targetDist = float.MaxValue;
    private int closetIndex = 0;
    private int targetIndex = 0;

    private static readonly int IsAttack = Animator.StringToHash("isAttack");
    private static readonly int DoSkill = Animator.StringToHash("doSkill");

    protected override void Start()
    {
        monsterMgr = MonsterManager.Instance;

        EntityData = DataManager.Instance.GameData.playerData;
        fsm.ChangeState(EStates.Init);

        JoystickEventBinding();

        base.Start();
    }

    private void JoystickEventBinding()
    {
        //dynamicJoystick.OnPointerDownAction += () => fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        dynamicJoystick.OnPointerDownAction += () =>
        {
            if(fsm.State != EStates.Skill)
                fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        };
        
        dynamicJoystick.OnPointerUpAction += () =>
        {
            if (TargetEntity == null || !TargetEntity.gameObject.activeSelf)
                fsm.ChangeState(EStates.Idle);
            else
                fsm.ChangeState(EStates.Track);
        };

        //staticJoystick.OnPointerDownAction += () => fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        staticJoystick.OnPointerDownAction += () =>
        {
            if(fsm.State != EStates.Skill)
                fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        };
        
        staticJoystick.OnPointerUpAction += () =>
        {
            if (TargetEntity == null || !TargetEntity.gameObject.activeSelf)
                fsm.ChangeState(EStates.Idle);
            else
                fsm.ChangeState(EStates.Track);
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
        if (IsDead)
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

    protected override void TrackFlow()
    {
        if (IsDead)
            return;

        if (monsterMgr.SpawnedMonsterList.Count > 0)
        {
            closetIndex = 0;
            targetIndex = -1;

            for (int i = 0; i < monsterMgr.SpawnedMonsterList.Count; i++)
            {
                Vector3 monsterPos = monsterMgr.SpawnedMonsterList[i].transform.position;
                curDist = Vector3.Distance(transform.position, monsterPos);

                if (Physics.Raycast(transform.position, monsterPos - transform.position,
                        EntityData.attackRange, targetLayer.value))
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

        var targetPosition = TargetEntity.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition,
            EntityData.moveSpeed * Time.deltaTime);

        RotateToTarget();
    }

    private void RotateToTarget()
    {
        Vector3 dir = TargetEntity.transform.position - transform.position;
        if (dir.sqrMagnitude != 0)
        {
            Quaternion dirQuat = Quaternion.LookRotation(dir);
            Quaternion moveQuat = Quaternion.Slerp(rigid.rotation, dirQuat, 0.3f);
            rigid.MoveRotation(moveQuat);
        }
    }

    protected override void AttackFlow()
    {
        if (!IsDead)
        {
            if (moveInputVec.sqrMagnitude != 0)
            {
                anim.SetBool(IsWalk, true);
                anim.SetBool(IsAttack, false);
                fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
            }

            if (HasTarget)
            {
                if (IsAttackable)
                {
                    if (!IsAttached)
                    {
                        Debug.Log("공격하려는데 멀어서 다시 track로");
                        fsm.ChangeState(EStates.Track);
                        return;
                    }
                    
                    lastAttackTime = Time.time;

                    RotateToTarget();

                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                        anim.SetBool(IsAttack, true);
                }
            }
            else
            {
                Debug.Log("not hasTarget");
                TargetEntity = null;
                fsm.ChangeState(EStates.Idle);
            }
        }
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
        Debug.Log("UseSkill :: " + skillType);
        
        fsm.ChangeState(EStates.Skill, StateTransition.Overwrite);

        int amount = DataManager.Instance.GameData.skillEffectAmounts[(int)skillType];
        int damageAmount = EntityData.attackPower * (100 + amount) / 100;
        switch (skillType)
        {
            case ESkillType.PhantomBlade:
                if (IsAttached)
                {
                    Debug.Log("Already Attached Attack, Add Blade Eft");
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
    }
}