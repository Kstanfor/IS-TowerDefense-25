using UnityEngine;

public class Turretv2 : MonoBehaviour
{
    // ───── EXISTING FIELDS ──────────────────────────────────────────────────
    public Transform target;
    public Enemy targetEnemy;

    [Header("General")]
    public float range = 15f;

    [Header("Use Bullets (default)")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Use Laser")]
    public bool useLaser = false;
    public int damageOverTime = 25;
    public float slowAmount = .5f;
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public Light impactLight;

    [Header("Unity Setup Fields")]
    public string enemyTag = "Enemy";
    public Transform PartToRotate;
    public float turnSpeed = 10f;
    public Transform firePoint;

    // ───── NEW ELEMENTAL VARIANT SETTINGS ───────────────────────────────────
    [Header("Elemental Variant")]
    public ElementType elementType = ElementType.Normal;
    // NEW: choose Fire/Water/Ice in Inspector
    public float statusDuration = 2f;
    // NEW: how long the effect lasts
    public float statusTickInterval = 1f;
    // NEW: only used by Fire
    public int statusDamage = 5;
    // NEW: per‐tick burn damage

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null && shortestDistance <= range)
            {
                target = nearestEnemy.transform;
                targetEnemy = nearestEnemy.GetComponent<Enemy>();
            }
            else
            {
                target = null;
            }
        }
    }

    void Update()
   {
        if (target == null)
        {
            if (target == null)
            {
                if (useLaser)
                {
                    if (lineRenderer.enabled)
                    {
                        lineRenderer.enabled = false;

                        impactEffect.Stop();
                        impactLight.enabled = false;
                    }

                }
            }

                return;
        }

        LockOnTarget();

            if (useLaser)
            { 
            Laser(); 
            }
            else
            {
                if (fireCountdown <= 0f)
                {
                    Shoot();
                    fireCountdown = 1f / fireRate;
                }
                fireCountdown -= Time.deltaTime;
            }
    }

    void LockOnTarget()
    {
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(PartToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        PartToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

    }

    void Laser()
    {
        targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
        targetEnemy.Slow(slowAmount);

        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;

            impactEffect.Play();
            impactLight.enabled = true;
        }

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, target.position);

        Vector3 dir = firePoint.position - target.position;

        impactEffect.transform.position = target.position + dir.normalized;

        impactEffect.transform.position = target.position;

    }

    // ───── MODIFIED: pass elemental data into the bullet ────────────────────
    void Shoot()
    {
        GameObject bulletGo = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bulletv2 bullet = bulletGo.GetComponent<Bulletv2>();

        if (bullet != null)
        {
            bullet.Seek(target);

            // NEW: hand off our chosen element + parameters
            bullet.element = elementType;
            bullet.statusDuration = statusDuration;
            bullet.statusTickInterval = statusTickInterval;
            bullet.statusDamage = statusDamage;
            bullet.slowAmount = slowAmount;
        }

        Debug.Log("Shooting!!!!!!!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}