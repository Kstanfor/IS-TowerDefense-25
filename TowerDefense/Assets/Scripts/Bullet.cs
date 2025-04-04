using System.Net.NetworkInformation;
using System.Security.Cryptography;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform target;

    public float speed = 70f;
    public float explosionRadius = 0f;
    public GameObject impactEffect; 

    public void Seek (Transform _target)
    {
        target = _target;
    }

    // Update is called once per frame
    void Update ()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);

    }

    void HitTarget ()
    {
        GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 5f);

        if (explosionRadius > 0f)
        {
            Explode();
        } else
        {
            Damage(target);
        }

        
        Destroy(gameObject);
        Debug.Log("We Got Em Cap!");
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Enemy")
            {
                Damage(collider.transform);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }


    void Damage(Transform enemy)
    {
        Destroy(enemy.gameObject);
    }
}
