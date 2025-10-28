using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Pickup Info")]
    public Pickup worldPickupPrefab;

    public enum AttackType { SingleShot, BurstFire, RapidFire }     // �ܹ�, ����, ����

    [Header("Attack")]
    public Camera cam;                      // �÷��̾� ī�޶�
    public AttackType attackType;           // ��� ���
    public Transform firePoint;             // �ѱ� ��ġ
    public LayerMask aimMask = ~0;          // ~0 <- ��� ���̾�
    public float maxRange = 100f;           // Ray �ִ� ��Ÿ�

    [Header("SingleShot")]
    public float sFireCooldown = 0.1f;

    [Header("BurstFire")]
    public float bFireCooldown = 0.4f;
    public float bInterval = 0.15f;
    public int bCount = 3;

    [Header("RapidFire")]
    public float rInterval = 0.2f;

    [Header("Ammo")]
    public GameObject bulletPrefab;     // �Ѿ� ������
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2;
    private bool isReloading;
    private float reloadTimer;


    private float cooldownTimer;

    private int bC;
    private float burstTimer;
    private bool isBursting;

    private float rapidTimer;

    // Start
    void Start()
    {
        currentAmmo = maxAmmo;

        if (!cam) cam = Camera.main;
    }

    // Update
    void Update()
    {
        // ����
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                currentAmmo = maxAmmo;
                isReloading = false;
            }

            return;
        }

        // �ܹ�, �����
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        // �����
        if (rapidTimer > 0f) rapidTimer -= Time.deltaTime;

        // ����
        if (isBursting)
        {
            burstTimer -= Time.deltaTime;

            if (burstTimer <= 0f)
            {
                FireBullet();

                bC--;

                if (bC <= 0) isBursting = false;
                else burstTimer = bInterval;
            }
        }
    }

    public void FireInput(bool toggle, bool hold)
    {
        if (isReloading) return;

        switch (attackType)
        {
            case AttackType.SingleShot:
                if (toggle) SingleShot();
                break;

            case AttackType.BurstFire:
                if (toggle) BurstFire();
                break;

            case AttackType.RapidFire:
                if (hold) RapidFire();
                break;
        }
    }

    // ���� ����
    public void ReloadInput(bool reloading)
    {
        if (!reloading || isReloading || isBursting) return;
        if (currentAmmo == maxAmmo) return;

        isReloading = true;
        reloadTimer = reloadTime;
    }

    void FireBullet()
    {
        if (!firePoint || !bulletPrefab) return;
        if (currentAmmo <= 0) return;

        // Ray ���� ���� = ��Ʈ��ĵ
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));      // ȭ�� �߾����� Ray �߻�
        Vector3 point;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, aimMask)) point = hit.point;
        else point = ray.GetPoint(maxRange);

        // �ѱ����� ������������
        Vector3 fireDir = (point - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(fireDir));

        currentAmmo--;
    }

    // �ܹ� ���
    void SingleShot()
    {
        if (cooldownTimer > 0f) return;
        if (currentAmmo <= 0) return;

        FireBullet();

        cooldownTimer = sFireCooldown;
    }

    // ���� ���
    void BurstFire()
    {
        if (isBursting) return;

        if (cooldownTimer > 0f) return;
        if (currentAmmo <= 0) return;

        FireBullet();

        bC = bCount - 1;
        if (bC > 0)
        {
            isBursting = true;
            burstTimer = bInterval;
        }

        cooldownTimer = bFireCooldown;
    }

    // ���� ���
    void RapidFire()
    {
        if (rapidTimer > 0f) return;
        if (currentAmmo <= 0) return;

        FireBullet();

        rapidTimer = rInterval;
    }
}
