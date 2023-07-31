using System;
using UnityEngine;

public class PhantomBlade : Projectile
{
    protected override void ReturnObject(float delay = 0.0f)
    {
        EffectManager.Instance.ReturnObj(EEffectType.PhantomBlade, gameObject, delay);
    }

    protected override void OnEnterProcess(Collider other)
    {
        if (other.TryGetComponent(out LivingEntity livingEntity))
        {
            livingEntity.ApplyDamage(dmgMsg);
            ReturnObject();
        }
    }
}
