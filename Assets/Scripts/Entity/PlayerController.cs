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

    [SerializeField, Range(0.1f, 10.0f)] private float moveSpeed = 5.0f;

    [SerializeField] private Vector2 moveInputVec;
    [SerializeField] private Vector3 moveVec;
    
    private static readonly int DoAttack = Animator.StringToHash("doAttack");

    protected override void Start()
    {
        base.Start();
        
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

    protected override void Control_FixedUpdate()
    {
        moveVec = new Vector3(moveInputVec.x, 0f, moveInputVec.y) * moveSpeed * Time.deltaTime;
        rigid.MovePosition(rigid.position + moveVec);

        if (moveVec.sqrMagnitude == 0)
            return;

        if (moveVec != Vector3.zero)
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
                    entityData.moveSpeed * Time.deltaTime);

            Vector3 dir = position - transform.position;
            if (dir != Vector3.zero)
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
            if (HasTarget && IsAttackable)
            {
                lastAttackTime = Time.time;

                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    anim.SetTrigger(DoAttack);
            }
        }
    }
}
