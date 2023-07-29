using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float moveVelocity;
    private float impactRange;
    private float lifeDistance;

    private Vector3 startPos;
    private DamageMessage dmgMsg;
    private LayerMask targetLayer;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var distance = Vector3.Distance(startPos, transform.position);
        if (distance >= lifeDistance)
            EffectManager.Instance.ReturnObj(EEffectType.Explosion, gameObject);
    }

    private void FixedUpdate()
    {
        rigid.velocity = transform.forward * moveVelocity;
    }

    public void SetupProjectile(DamageMessage dmg, LayerMask layerMask, float velocity, float distance, float range)
    {
        startPos = transform.position;
        
        dmgMsg = dmg;
        moveVelocity = velocity;
        targetLayer = layerMask;
        impactRange = range;
        lifeDistance = distance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Utility.MaskToLayer(targetLayer))
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, impactRange, targetLayer.value);
            if (cols.Length > 0)
            {
                AudioManager.Instance.PlaySFX(ESFXName.Explosion);
                
                var explosion = EffectManager.Instance.InstantiateObj(EEffectType.Explosion);
                explosion.transform.position = transform.position;
                EffectManager.Instance.ReturnObj(EEffectType.Explosion, explosion, 2.0f);

                foreach (var col in cols)
                {
                    if (col.TryGetComponent(out LivingEntity livingEntity))
                        livingEntity.ApplyDamage(dmgMsg);
                }

                EffectManager.Instance.ReturnObj(EEffectType.FireBall, gameObject);
            }
        }
    }
}