using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;

public class PlayerController : Entity
{
    [SerializeField] private FloatingJoystick joystick;

    [SerializeField, Range(0.1f, 10.0f)] private float moveSpeed = 5.0f;

    private Vector3 moveVec;
    
    private static readonly int DoAttack = Animator.StringToHash("doAttack");

    protected override void Start()
    {
        base.Start();

        joystick.OnPointerDownAction += () => fsm.ChangeState(EStates.Control, StateTransition.Overwrite);

        joystick.OnPointerUpAction += () =>
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
        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        moveVec = new Vector3(x, 0f, z) * moveSpeed * Time.deltaTime;
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
                    entityData.MoveSpeed * Time.deltaTime);

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
