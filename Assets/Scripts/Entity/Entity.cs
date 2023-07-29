using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonsterLove.StateMachine;
using NaughtyAttributes;
using UnityEngine.Serialization;

public abstract class Entity : LivingEntity
{
    protected GameManager gameMgr;
    
    public enum EStates
    {
        Init,
        Idle,
        Control,
        Track,
        Attack,
        Skill,
        Die
    }

    protected StateMachine<EStates> fsm;

    [HorizontalLine(color: EColor.Red), BoxGroup("# Status Settings")]
    [SerializeField] private EntityData entityData;

    public EntityData EntityData
    {
        get { return entityData; }
        protected set { entityData = value; }
    }

    [HorizontalLine(color: EColor.Orange), BoxGroup("# Status Settings"), SerializeField]
    private Slider hpBar;
    
    // Attack
    [HorizontalLine(color: EColor.Yellow), BoxGroup("# Attack Settings"), SerializeField]
    protected LayerMask targetLayer;

    [BoxGroup("# Attack Settings"), SerializeField]
    private LivingEntity targetEntity;
    public LivingEntity TargetEntity
    {
        get { return targetEntity; }
        protected set { targetEntity = value; }
    }

    protected bool HasTarget => TargetEntity != null && !TargetEntity.IsDead;

    protected float lastAttackTime = 0.0f;

    protected bool IsAttackable =>
        Time.time >= lastAttackTime + EntityData.attackPerSecond;

    public bool IsAttached =>
        TargetEntity != null && Vector3.Distance(TargetEntity.transform.position, transform.position) <= entityData.attackRange;
    
    [HorizontalLine(color: EColor.Green), BoxGroup("# Material Settings"), SerializeField]
    protected List<MeshRenderer> equipMeshRendererList = new List<MeshRenderer>();

    [BoxGroup("# Material Settings"), SerializeField]
    protected SkinnedMeshRenderer bodyMeshRenderer;
    [FormerlySerializedAs("mats")] [BoxGroup("# Material Settings"), SerializeField]
    protected Material[] defaultMats;

    [BoxGroup("# Material Settings"), SerializeField]
    protected Material impactMat;
    
    protected Animator anim;
    protected Rigidbody rigid;

    protected static readonly int IsWalk = Animator.StringToHash("isWalk");
    protected static readonly int DoDie = Animator.StringToHash("doDie");
    protected static readonly int IsAttack = Animator.StringToHash("isAttack");

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();

        fsm = new StateMachine<EStates>(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        fsm.ChangeState(EStates.Init);
    }

    protected virtual void Start()
    {
        gameMgr = GameManager.Instance;
        
        DeathAction += () =>
        {
            fsm.ChangeState(EStates.Die);
            anim.SetTrigger(DoDie);
            gameObject.layer = LayerMask.NameToLayer("Ignore");
        };

        ChangedHpValueAction += () => hpBar.value = (float)CurrentHp / originHp;
    }

    public void UpdateHpUI()
    {
        originHp = entityData.healthPoint + entityData.increaseHealthPoint;
        
        hpBar.maxValue = 1.0f;
        hpBar.value = (float)CurrentHp / originHp;
    }

    protected virtual void Init_Enter()
    {
        anim.ResetTrigger(DoDie);
        anim.SetBool(IsWalk, false);
        anim.SetBool(IsAttack, false);
        gameObject.layer = LayerMask.NameToLayer(entityData.entityType.ToString());
        
        hpBar.gameObject.SetActive(true);
        
        originHp = entityData.healthPoint + entityData.increaseHealthPoint;
        CurrentHp = originHp;
        
        UpdateHpUI();

        if (TargetEntity == null)
            fsm.ChangeState(EStates.Idle);
        else
            fsm.ChangeState(EStates.Track);
    }

    protected virtual void Idle_Enter()
    {
        anim.SetBool(IsWalk, false);
    }

    /// <summary>
    /// idle 상태에서는 target이 존재하는지 확인한다.
    /// 만약 target이 있으면 거리에 따라 attack을 하거나 track한다.
    /// </summary>
    protected virtual void Idle_Update()
    {
        if (!gameMgr.IsPlaying)
            return;
        
        if (HasTarget)
        {
            if(IsAttackable && IsAttached)
                fsm.ChangeState(EStates.Attack);
            else
                fsm.ChangeState(EStates.Track);
        }
    }

    protected virtual void Idle_Exit()
    {
        
    }

    protected virtual void Control_Enter()
    {
        anim.SetBool(IsWalk, true);
    }

    protected virtual void Control_FixedUpdate()
    {
        
    }

    protected virtual void Control_Exit()
    {
       
    }
    
    protected void RotateToTarget()
    {
        Vector3 dir = TargetEntity.transform.position - transform.position;
        if (dir.sqrMagnitude != 0)
        {
            Quaternion dirQuat = Quaternion.LookRotation(dir);
            Quaternion moveQuat = Quaternion.Slerp(rigid.rotation, dirQuat, 0.3f);
            rigid.MoveRotation(moveQuat);
        }
    }

    protected virtual void Track_Enter()
    {
        anim.SetBool(IsWalk, true);
    }

    /// <summary>
    /// 만약 targetEntity가 있고 살아있다면
    /// 공격 범위에 들어왔을때 attack 상태로 전환하고, 범위 밖이라면 추적한다.
    /// </summary>
    protected abstract void Track_Update();

    protected virtual void Track_Exit()
    {
        rigid.velocity = Vector3.zero;
        anim.SetBool(IsWalk, false);
    }

    protected virtual void Attack_Enter()
    {
        if(!HasTarget)
            fsm.ChangeState(EStates.Idle);
    }

    protected virtual void Attack_Update()
    {
        if(IsDead || !gameMgr.IsPlaying)
            return;
        
        if (HasTarget)
        {
            RotateToTarget();
                
            if (IsAttackable)
            {
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
    
    protected virtual void Attack_Exit()
    {
        anim.SetBool(IsAttack, false);
    }

    protected virtual void OnAttack1Trigger()
    {
        AttackTargetEntity();
        anim.SetBool(IsAttack, false);
        
        if (targetEntity.IsDead)
        {
            targetEntity = null;
            fsm.ChangeState(EStates.Idle);
        }
        else
        {
            fsm.ChangeState(EStates.Attack);
        }
    }

    protected void AttackTargetEntity(int damage = 0)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, TargetEntity.transform.position - transform.position, out hit,
                entityData.attackRange, targetLayer))
        {
            if (hit.collider.gameObject != TargetEntity.gameObject)
            {
                if (hit.collider.TryGetComponent(out LivingEntity otherEntity))
                    TargetEntity = otherEntity;
            }

            DamageMessage dmgMsg = new DamageMessage(this.gameObject, damage > 0 ? damage : entityData.attackPower, hit.point);
            TargetEntity.ApplyDamage(dmgMsg);

            if(entityData.entityType == EEntityType.Player)
                AudioManager.Instance.PlaySFX(ESFXName.PlayerAttack);
            if(entityData.entityType == EEntityType.Monster)
                AudioManager.Instance.PlaySFX(ESFXName.MonsterAttack);
        }
    }

    protected virtual void Skill_Enter()
    {
        
    }

    protected virtual void Skill_Exit()
    {
        
    }

    protected virtual void Die_Enter()
    {
        SwapMaterial(EMaterialType.Default);
        hpBar.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public override void ApplyDamage(DamageMessage dmgMsg)
    {
        StartCoroutine(ImpactMaterialCo());
        base.ApplyDamage(dmgMsg);
    }

    [ContextMenu("Set Equip Materials")]
    protected void SetEquipMaterial()
    {
        equipMeshRendererList.Clear();
        var meshRenders = GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRen in meshRenders)
            equipMeshRendererList.Add(meshRen);

        bodyMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public IEnumerator ImpactMaterialCo()
    {
        SwapMaterial(EMaterialType.Flash);

        yield return new WaitForSeconds(0.2f);
        
        SwapMaterial(EMaterialType.Default);
    }

    public void SwapMaterial(EMaterialType matType)
    {
        if (matType == EMaterialType.Default)
        {
            if (equipMeshRendererList.Count > 0)
            {
                foreach (var meshRen in equipMeshRendererList)
                    meshRen.sharedMaterial = defaultMats[0];
                bodyMeshRenderer.sharedMaterial = defaultMats[1];
            }
            else
            {
                bodyMeshRenderer.sharedMaterial = defaultMats[0];
            }
        }
        else
        {
            if (equipMeshRendererList.Count > 0)
            {
                foreach (var meshRen in equipMeshRendererList)
                    meshRen.sharedMaterial = impactMat;
            }
    
            bodyMeshRenderer.sharedMaterial = impactMat;
        }
    }
}