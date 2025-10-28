using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Stat")]
    public float speed = 15f;
    public float lifeTime = 8f;

    private int damage;
    private Vector3 moveDir;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(int dmg, Vector3 dir)
    {
        damage = dmg;
        moveDir = dir.normalized;
    }

    void Update()
    {
        // 앞으로 직선 이동
        transform.position += moveDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        Destroy(gameObject);
    }
}
