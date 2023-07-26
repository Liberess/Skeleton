using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonMobileUI.Scripts;
using MonsterLove.StateMachine;

public class PlayerController : Entity
{
    private MonsterManager monsterMgr;
    
    [SerializeField] private StaticJoystickController staticJoystick;
    [SerializeField] private DynamicJoystickController dynamicJoystick;
    
    private Vector2 moveInputVec;
    private Vector3 moveVec;

    private float curDist = 0.0f;
    private float closetDist = float.MaxValue;
    private float targetDist = float.MaxValue;
    private int closetIndex = 0;
    private int targetIndex = 0;
    
    private static readonly int IsAttack = Animator.StringToHash("isAttack");

    protected override void Start()
    {
        monsterMgr = MonsterManager.Instance;
        
        EntityData = DataManager.Instance.GameData.playerData;
        fsm.ChangeState(EStates.Init);
        
        dynamicJoystick.OnPointerDownAction += () => fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        dynamicJoystick.OnPointerUpAction += () =>
        {
            if (targetEntity == null || !targetEntity.gameObject.activeSelf)
                fsm.ChangeState(EStates.Idle);
            else
                fsm.ChangeState(EStates.Track);
        };
        
        staticJoystick.OnPointerDownAction += () => fsm.ChangeState(EStates.Control, StateTransition.Overwrite);
        staticJoystick.OnPointerUpAction += () =>
        {
            if (targetEntity == null || !targetEntity.gameObject.activeSelf)
                fsm.ChangeState(EStates.Idle);
            else
                fsm.ChangeState(EStates.Track);
        };
        
        base.Start();
    }

    private void Update()
    {
        fsm.Driver.Update?.Invoke();
    }

    private void FixedUpdate()
    {
        fsm.Driver.FixedUpdate?.Invoke();
    }

    protected override void Control_FixedUpdate()
    {
        if(IsDead)
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

    public void OnMoveInput(Vector2 moveInput) =>  moveInputVec = moveInput;

    protected override void TrackFlow()
    {
        if(IsDead)
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
                        EntityData.attackRange, targetLayer))
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

            targetEntity = monsterMgr.SpawnedMonsterList[targetIndex];

            closetDist = float.MaxValue;
            targetDist = float.MaxValue;
        }
        
        var position = targetEntity.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, position,
            EntityData.moveSpeed * Time.deltaTime);

        Vector3 dir = position - transform.position;
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
                    lastAttackTime = Time.time;

                    if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                        anim.SetBool(IsAttack, true);
                }
            }
            else
            {
                targetEntity = null;
                fsm.ChangeState(EStates.Idle);
            }
        }
    }

    protected override void Attack_Exit()
    {
        anim.SetBool(IsAttack, false);
        base.Attack_Exit();
    }
}
