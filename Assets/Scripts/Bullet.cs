using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletType { Normal, Bounce, Explosive }

    [Header("Type")]
    public BulletType bulletType;       // 드롭다운 선택

    [Header("Stat")]
    public int damage;                  // 대미지
    public float speed;                 // 총알 속도
    public float lifeTime = 4f;         // 총알 생존 시간

    [Header("Bounce")]
    public int maxBounceCount = 4;      // 튕기는 횟수
    private int bounceCount = 0;        // 현재 튕긴 횟수

    [Header("Explosive")]
    public int explosionDamage;         // 폭파 대미지
    public float explosiveRadius = 4f;  // 폭발 범위

    private Rigidbody rb;

    // Awake
    private void Awake()
    {
        // 콜라이더가 꼭 필요하며 is Trigger는 false로

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();  // Rigidbody가 없다면 추가해주기

        // isKinematic 가 true면 false로 변경
        if (rb.isKinematic) rb.isKinematic = false;

        // 중력 끄기
        if (rb.useGravity) rb.useGravity = false;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Start
    void Start()
    {
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)              // Collision = 방향을 받아오기 위해
    {
        // collision에서 collider를 받아오고 거기서 Enemy를 찾아오기 ( 이 GameObject에 없다면 부모들까지 올라간다 )
        Enemy enemy = collision.collider.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        switch (bulletType)
        {
            case BulletType.Normal:
                Destroy(gameObject);
                return;

            case BulletType.Bounce:
                AttackBounce(collision);
                return;

            case BulletType.Explosive:
                AttackExplosive(collision.GetContact(0).point);     // 처음에 닿은 지점
                return;
        }
    }

    void AttackBounce(Collision collision)
    {
        // contact.point = 실제로 닿은 월드 좌표
        // contact.normal = 닿은 표면이 밀어내는 방향 벡터
        ContactPoint contact = collision.GetContact(0);

        Vector3 inDir = rb.velocity.normalized;
        Vector3 normalB = contact.normal;
        Vector3 outDir = Vector3.Reflect(inDir, normalB).normalized;    // 반사

        transform.position = contact.point + normalB * 0.01f;           // 벽에 박힘 방지
        transform.rotation = Quaternion.LookRotation(outDir);          // 총알을 바라보는 방향으로 회전

        rb.velocity = outDir * speed;

        bounceCount++;
        if (bounceCount >= maxBounceCount)
        {
            Destroy(gameObject);
        }
    }

    void AttackExplosive(Vector3 point)
    {
        Collider[] hit = Physics.OverlapSphere(point, explosiveRadius); // 구 안에 있는 모든 Collider

        foreach (Collider col in hit)
        {
            Enemy enemyE = col.GetComponentInParent<Enemy>();
            if (enemyE != null)
            {
                enemyE.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }
}
