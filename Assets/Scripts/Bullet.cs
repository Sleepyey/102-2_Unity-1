using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletType { Normal, Bounce, Explosive }

    [Header("Type")]
    public BulletType bulletType;       // ��Ӵٿ� ����

    [Header("Stat")]
    public int damage;                  // �����
    public float speed;                 // �Ѿ� �ӵ�
    public float lifeTime = 4f;         // �Ѿ� ���� �ð�

    [Header("Bounce")]
    public int maxBounceCount = 4;      // ƨ��� Ƚ��
    private int bounceCount = 0;        // ���� ƨ�� Ƚ��

    [Header("Explosive")]
    public int explosionDamage;         // ���� �����
    public float explosiveRadius = 4f;  // ���� ����

    private Rigidbody rb;

    // Awake
    private void Awake()
    {
        // �ݶ��̴��� �� �ʿ��ϸ� is Trigger�� false��

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();  // Rigidbody�� ���ٸ� �߰����ֱ�

        // isKinematic �� true�� false�� ����
        if (rb.isKinematic) rb.isKinematic = false;

        // �߷� ����
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

    private void OnCollisionEnter(Collision collision)              // Collision = ������ �޾ƿ��� ����
    {
        // collision���� collider�� �޾ƿ��� �ű⼭ Enemy�� ã�ƿ��� ( �� GameObject�� ���ٸ� �θ����� �ö󰣴� )
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
                AttackExplosive(collision.GetContact(0).point);     // ó���� ���� ����
                return;
        }
    }

    void AttackBounce(Collision collision)
    {
        // contact.point = ������ ���� ���� ��ǥ
        // contact.normal = ���� ǥ���� �о�� ���� ����
        ContactPoint contact = collision.GetContact(0);

        Vector3 inDir = rb.velocity.normalized;
        Vector3 normalB = contact.normal;
        Vector3 outDir = Vector3.Reflect(inDir, normalB).normalized;    // �ݻ�

        transform.position = contact.point + normalB * 0.01f;           // ���� ���� ����
        transform.rotation = Quaternion.LookRotation(outDir);          // �Ѿ��� �ٶ󺸴� �������� ȸ��

        rb.velocity = outDir * speed;

        bounceCount++;
        if (bounceCount >= maxBounceCount)
        {
            Destroy(gameObject);
        }
    }

    void AttackExplosive(Vector3 point)
    {
        Collider[] hit = Physics.OverlapSphere(point, explosiveRadius); // �� �ȿ� �ִ� ��� Collider

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
