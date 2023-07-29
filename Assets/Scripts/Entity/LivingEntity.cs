using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class LivingEntity : MonoBehaviour
{
    public int originHp { get; protected set; } = 100;
    
    [HorizontalLine(color: EColor.Red), BoxGroup("# LivingEntity"), SerializeField, ProgressBar("Health", "originHp", EColor.Red)]
    private int currentHp;
    public int CurrentHp
    {
        get => currentHp;

        set
        {
            currentHp = value;

            if (currentHp > originHp)
                currentHp = originHp;
            else if (currentHp <= 0)
                Die();

            ChangedHpValueAction?.Invoke();
        }
    }

    [BoxGroup("# LivingEntity"), SerializeField]
    protected float minTimeBetDamaged = 0.1f;
    private float lastDamagedTime;

    protected bool IsDamageable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged)
                return true;

            return false;
        }
    }

    [BoxGroup("# LivingEntity"), ShowNonSerializedField]
    private bool isDead; 
    public bool IsDead
    {
        get => isDead;
        protected set => isDead = value;
    }

    public UnityAction DeathAction;
    public UnityAction ChangedHpValueAction;

    protected virtual void OnEnable()
    {
        IsDead = false;
    }

    public virtual void ApplyDamage(DamageMessage dmgMsg)
    {
        if (IsDead || !IsDamageable)
            return;

        lastDamagedTime = Time.time;
        CurrentHp -= dmgMsg.damageAmount;
    }

    public virtual void RecoveryHealthPoint(int cureAmount = 0)
    {
        if (IsDead)
            return;

        CurrentHp += cureAmount;
    }

    public virtual void Die()
    {
        DeathAction?.Invoke();
        IsDead = true;
    }

    public void SetOriginHp(int newHp)
    {
        originHp = newHp;
        currentHp = originHp;
    }
}