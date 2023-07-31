using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected float moveVelocity;
    protected float impactRange;
    protected float lifeDistance;

    protected Vector3 startPos;
    protected DamageMessage dmgMsg;
    protected LayerMask targetLayer;

    protected Rigidbody rigid;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        GetComponent<ParticleSystem>().Play();
        GetComponent<SphereCollider>().enabled = true;
    }

    protected virtual void Update()
    {
        var distance = Vector3.Distance(startPos, transform.position);
        if (distance >= lifeDistance)
            ReturnObject(0.0f);
    }

    protected virtual void FixedUpdate()
    {
        rigid.velocity = transform.forward * moveVelocity;
    }

    protected abstract void ReturnObject(float delay = 0.0f);

    public void SetupProjectile(DamageMessage dmg, LayerMask layerMask, float velocity, float distance, float range)
    {
        startPos = transform.position;
        
        dmgMsg = dmg;
        moveVelocity = velocity;
        targetLayer = layerMask;
        impactRange = range;
        lifeDistance = distance;
    }

    protected abstract void OnEnterProcess(Collider other);

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Utility.MaskToLayer(targetLayer))
            OnEnterProcess(other);
    }
}