using System.Security.Cryptography;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform target;

    public float speed = 70f;
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

    }

    void HitTarget ()
    {
        GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 2f);

        Destroy(target.gameObject);
        Destroy(gameObject);
        Debug.Log("We Got Em Cap!");
    }
}
