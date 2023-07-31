using UnityEngine;

public class FireBall : Projectile
{
    protected override void ReturnObject(float delay = 0.0f)
    {
        EffectManager.Instance.ReturnObj(EEffectType.Explosion, gameObject, delay);
    }

    protected override void OnEnterProcess(Collider other)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, impactRange, targetLayer.value);
        if (cols.Length > 0)
        {
            AudioManager.Instance.PlaySFX(ESFXName.Explosion);
                
            var explosion = EffectManager.Instance.InstantiateObj(EEffectType.Explosion);
            explosion.transform.position = transform.position;

            foreach (var col in cols)
            {
                if (col.TryGetComponent(out LivingEntity livingEntity))
                    livingEntity.ApplyDamage(dmgMsg);
            }
            
            ReturnObject();
        }
    }
}
