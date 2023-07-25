using UnityEngine;

public struct DamageMessage
{
    public GameObject damager;
    public int damageAmount;
    public Vector3 hitPoint;

    public DamageMessage(GameObject _damager, int _damageAmount, Vector3 _hitPoint)
    {
        damager = _damager;
        damageAmount = _damageAmount;
        hitPoint = _hitPoint;
    }

    public DamageMessage(GameObject _damager, int _damageAmount)
    {
        damager = _damager;
        damageAmount = _damageAmount;
        hitPoint = Vector3.zero;
    }
}

public interface IDamageable
{
    void ApplyDamage(DamageMessage dmgMsg);
}