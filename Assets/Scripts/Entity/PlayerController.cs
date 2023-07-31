using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    private static readonly int DoDisabledAttack = Animator.StringToHash("doDisabledAttack");

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

    private void OnJoystickDown()
    {
        if (fsm.State != EStates.Skill && !isDashing)
        {
            isControlling = true;
            anim.SetBool(IsAttack, false);
            anim.SetTrigger(DoDisabledAttack);
            fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        }
    }

    private void OnJoystickUp()
    {
        isControlling = false;
        if (HasTarget)
            fsm.ChangeState(EStates.Track);
        else
            fsm.ChangeState(EStates.Idle);
    }
    
    private void JoystickEventBinding()
    {
        dynamicJoystick.OnPointerDownAction += OnJoystickDown;
        dynamicJoystick.OnPointerUpAction += OnJoystickUp;

        staticJoystick.OnPointerDownAction += OnJoystickDown;
        staticJoystick.OnPointerUpAction += OnJoystickUp;
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

    protected override void Init_Enter()
    {
        TargetEntity = null;
        base.Init_Enter();
    }

    protected override void Control_Update()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;

        moveVec = new Vector3(moveInputVec.x, 0f, moveInputVec.y) * EntityData.moveSpeed * Time.deltaTime;
    }

    protected override void Control_FixedUpdate()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;
        
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
        if (IsDead || !gameMgr.IsPlaying)
            return;
        
        // 플레이어의 경우에는 복수의 target이 존재하기에,
        // 매번 가장 가까운 target을 확인해야 한다.
        FindNearestMonster().Forget();
        
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
    private async UniTaskVoid FindNearestMonster()
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
            
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    protected override void Track_Enter()
    {
        DisableAttackTrail();
        base.Track_Enter();
    }

    protected override void Track_Update()
    {
        if (IsDead || !gameMgr.IsPlaying)
            return;
        
        FindNearestMonster().Forget();
        
        if (HasTarget)
        {
            if (IsAttackable && IsAttached)
            {
                RotateToTarget();
                fsm.ChangeState(EStates.Attack);
            }

            if (!IsAttached && !isControlling)
            {
                var targetPosition = TargetEntity.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                    EntityData.moveSpeed * Time.deltaTime);
                RotateToTarget();
            }
            
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        else
        {
            fsm.ChangeState(EStates.Idle);
        }
    }

    protected override void Attack_Update()
    {
        if (isControlling && moveInputVec.sqrMagnitude != 0)
        {
            anim.SetBool(IsWalk, true);
            anim.SetBool(IsAttack, false);
            fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
            return;
        }

        if (HasTarget)
        {
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
                
                if(anim.GetBool(IsAttack))
                    anim.SetBool(IsAttack, false);
                anim.SetBool(IsAttack, true);
                
                RotateToTarget();
            }
            else
            {
                anim.SetBool(IsWalk, false);
                anim.SetBool(IsAttack, false);
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
        
        if(!HasTarget || !IsAttached)
            fsm.ChangeState(EStates.Idle);
    }

    protected override void Attack_Exit()
    {
        DisableAttackTrail();
        base.Attack_Exit();
    }

    protected override void Skill_Enter()
    {
        anim.SetBool(IsAttack, false);
    }

    protected override void Skill_Exit()
    {
        anim.SetBool(IsAttack, false);
    }

    public void UseSkill(ESkillType skillType)
    {
        fsm.ChangeState(EStates.Skill, StateTransition.Overwrite);
        
        int amount = DataManager.Instance.GameData.skillEffectAmounts[(int)skillType];
        int damageAmount = (EntityData.attackPower + EntityData.increaseAttackPower) * (100 + amount) / 100;

        if (skillType == ESkillType.Recovery)
        {
            AudioManager.Instance.PlaySFX(ESFXName.Recovery);
            fsm.ChangeState(EStates.Attack);
            RecoveryHealthPoint(amount);
        }
        else
        {
            anim.SetTrigger(DoSkill);
            
            SkillSO skillSo = DataManager.Instance.GetSkillSO(skillType);
            DamageMessage dmgMsg = new DamageMessage(this.gameObject, damageAmount);
            
            EEffectType targetType;
            if (skillType == ESkillType.PhantomBlade)
            {
                targetType = EEffectType.PhantomBlade;
                AudioManager.Instance.PlaySFX(ESFXName.Blade);
            }
            else
            {
                targetType = EEffectType.FireBall;
                AudioManager.Instance.PlaySFX(ESFXName.FireBall);
            }
                
            Vector3 effectSpawnOffset = Vector3.up;
            var skill = EffectManager.Instance.InstantiateObj(targetType);
            skill.transform.position = transform.position + effectSpawnOffset;
            
            // FireBall의 위치에서 Target의 위치까지의 방향 계산
            Vector3 targetPos = new Vector3(TargetEntity.transform.position.x, skill.transform.position.y, TargetEntity.transform.position.z);
            Vector3 targetDir = (targetPos - skill.transform.position).normalized;
                
            // FireBall이 계산된 방향을 보도록 설정
            skill.transform.rotation = Quaternion.LookRotation(targetDir);
                
            skill.GetComponent<Projectile>().SetupProjectile(dmgMsg, targetLayer, skillSo.projectileVelocity,
                skillSo.projectileDistance, skillSo.skillImpactRange);
        }
    }

    public void OnDisableUseSkill()
    {
        fsm.ChangeState(EStates.Attack);
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

    private void OnApplicationFocus(bool hasFocus)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if(hasFocus)
                fsm.ChangeState(EStates.Track);
            else
                fsm.ChangeState(EStates.Idle);
        }
    }
}