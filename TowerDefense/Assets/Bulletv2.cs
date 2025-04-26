using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulletv2 : MonoBehaviour
{
    // ───── NEW FIELDS ─────────────────────────────────────────────────────
    public ElementType element = ElementType.Normal;
    // NEW: which element this bullet carries
    public float statusDuration = 2f;
    // NEW: how long the status effect lasts
    public float statusTickInterval = 1f;
    // NEW: interval between ticks (only used by Fire)
    public int statusDamage = 5;
    // NEW: damage per tick for Fire
    public float slowAmount = 0.5f;
    // NEW: fraction to slow for Water

    // ───── EXISTING FIELDS ──────────────────────────────────────────────────
    public Transform target;
    public float speed = 70f;
    public int damage = 50;
    public float explosionRadius = 0f;
    public GameObject impactEffect;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distThisFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 5f);

        if (explosionRadius > 0f)
            Explode();
        else
            Damage(target);

        Destroy(gameObject);
        Debug.Log("We Got Em Cap!");
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
            if (col.CompareTag("Enemy"))
                Damage(col.transform);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

   
    void Damage(Transform enemy)
    {
        Enemyv2 e = enemy.GetComponent<Enemyv2>();
        if (e == null) return;

        // 1) Instant hit damage (unchanged)
        //e.TakeDamage(damage);
        float multiplier = ElementalEffect.GetMultiplier(element, e.elementType);
        float totalDamage = damage * multiplier;
        Debug.Log($"[Bullet] {element}→{e.elementType}: ×{multiplier} → dmg={totalDamage}");
        e.TakeDamage(totalDamage);

        // 2) NEW: apply status effect based on element
        switch (element)
        {
            case ElementType.Fire:
                // Burn damage over time
                Debug.Log("[Bullet] applying Fire dot");
                e.StartCoroutine(e.Burn(statusDamage, statusDuration, statusTickInterval));
                break;

            case ElementType.Water:
                // Slow movement for duration
                Debug.Log($"[Bullet] Applying Water slow: pct={slowAmount}, dur={statusDuration}");
                e.StartCoroutine(e.SlowRoutine(slowAmount, statusDuration));
                break;

            case ElementType.Ice:
                // Freeze (zero speed) for duration
                Debug.Log("[Bullet] Applying Ice freeze");
                e.StartCoroutine(e.FreezeRoutine(statusDuration));
                break;

            case ElementType.Normal:
            default:
                // no extra effect
                break;
        }
    }
}
