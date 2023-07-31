using UnityEngine;

public class FireBall : Projectile
{
    protected override void ReturnObject(float delay = 0.0f)
    {
        EffectManager.Instance.ReturnObj(EEffectType.FireBall, gameObject, delay);
    }

    protected override void OnEnterProcess(Collider other)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, impactRange, targetLayer.value);
        if (cols.Length > 0)
        {
            AudioManager.Instance.PlaySFX(ESFXName.Explosion);
                
            var explosion = EffectManager.Instance.InstantiateObj(EEffectType.Explosion);
            explosion.transform.position = transform.position;
            explosion.GetComponent<ParticleSystem>().Play();

            foreach (var col in cols)
            {
                if (col.TryGetComponent(out LivingEntity livingEntity))
                    livingEntity.ApplyDamage(dmgMsg);
            }
            
            ReturnObject();

            GetComponent<SphereCollider>().enabled = false;
            GetComponent<ParticleSystem>().Stop();
            EffectManager.Instance.ReturnObj(EEffectType.Explosion, explosion, 1.5f);
            ReturnObject(2.1f);
        }
    }
}
