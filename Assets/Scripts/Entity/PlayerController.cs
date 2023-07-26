using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonMobileUI.Scripts;
using MonsterLove.StateMachine;

public class PlayerController : Entity
{
    [SerializeField] private StaticJoystickController staticJoystick;
    [SerializeField] private DynamicJoystickController dynamicJoystick;
    
    private Vector2 moveInputVec;
    private Vector3 moveVec;
    
    private static readonly int DoAttack = Animator.StringToHash("doAttack");

    protected override void Start()
    {
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
        
        
        if (targetEntity == null)
        {
            fsm.ChangeState(EStates.Idle);
        }
        else
        {
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
    }

    protected override void AttackFlow()
    {
        if (!IsDead)
        {
            if (HasTarget)
            {
                if (IsAttackable)
                {
                    lastAttackTime = Time.time;

                    if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                        anim.SetTrigger(DoAttack);
                }
            }
            else
            {
                targetEntity = null;
            }
        }
    }
}
