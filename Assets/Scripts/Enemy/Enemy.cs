using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // EnemySO ScriptableObject 넣어주기
    [SerializeField] EnemySO enemySO;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Attack")]
    public EnemyAttackType enemyAttackType;
    public int attackDamage;
    public float attackSpeed;
    public float attackCooldown;
    public float attackRange;

    private float attackCool;

    [Header("Rapid")]
    public float rapidTimer;

    [Header("Stat")]
    public int hp;

    [Header("Move")]
    public EnemyMoveType enemyMoveType;
    public float moveSpeed;

    private Vector3 moveDir;
    private float moveChangeTimer;
    private float moveChangeInterval;

    private Transform player;       // 플레이어 추적용

    // Awake
    private void Awake()
    {
        bulletPrefab = enemySO.bulletPrefab;

        enemyAttackType = enemySO.enemyAttackType;
        attackDamage = enemySO.attackDamageSO;
        attackSpeed = enemySO.attackSpeedSO;
        attackCooldown = enemySO.attackCooldownSO;
        attackRange = enemySO.attackRangeSO;

        rapidTimer = enemySO.rapidTimerSO;

        hp = enemySO.hpSO;
        enemyMoveType = enemySO.enemyMoveType;
        moveSpeed = enemySO.moveSpeedSO;
    }

    // Start
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        moveDir = Random.insideUnitSphere;
        moveDir.y = 0f;
        moveDir.Normalize();
        moveChangeTimer = moveChangeInterval;
    }

    // Update
    void Update()
    {
        if (player == null) return;

        if (attackCool > 0f) attackCool -= Time.deltaTime;
        if (rapidTimer > 0f) rapidTimer -= Time.deltaTime;

        Move();
        Attack();
    }

    void Move()
    {
        switch (enemyMoveType)
        {
            case EnemyMoveType.MoveTarget:
                MovePlayer();
                break;

            case EnemyMoveType.MoveT:
                MoveT();
                break;

            case EnemyMoveType.MoveF:
                break;
        }
    }

    void MovePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist > 0.1f)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, 10f * Time.deltaTime);
            }
        }
    }

    void MoveT()
    {
        moveChangeTimer -= Time.deltaTime;
        if (moveChangeTimer <= 0f)
        {
            // 방향 갱신
            moveDir = Random.insideUnitSphere;
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude < 0.0001f) moveDir = Vector3.forward;
            moveDir.Normalize();

            moveChangeTimer = moveChangeInterval;
        }

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 이동 방향으로 천천히 회전
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }
    }

    void Attack()
    {
        float distPlayer = Vector3.Distance(transform.position, player.position);
        if (distPlayer > attackRange) return;

        // 플레이어 쪽으로 몸 수평 돌리기
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }

        switch (enemyAttackType)
        {
            case EnemyAttackType.SingleShot:
                // 단발: 쿨마다 한 발
                if (attackCool <= 0f)
                {
                    FireSingle();
                    attackCool = attackCooldown;
                }
                break;

            case EnemyAttackType.RapidFire:
                if (rapidTimer <= 0f)
                {
                    FireSingle();
                    rapidTimer = attackSpeed;
                }
                break;

            case EnemyAttackType.Spray:
                if (attackCool <= 0f)
                {
                    FireSpray();
                    attackCool = attackCooldown;
                }
                break;
        }
    }

    void FireSingle()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 dir = (player.position - firePoint.position).normalized;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir, Vector3.up));

        EnemyBullet eb = b.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.Init(attackDamage, dir);
        }
    }

    void FireSpray()
    {
        if (bulletPrefab == null || firePoint == null) return;

        int count = 5;
        float spreadAngle = 15f;

        for (int i = 0; i < count; i++)
        {
            // 기본 방향 = 플레이어
            Vector3 baseDir = (player.position - firePoint.position).normalized;

            // 살짝 무작위 각도로
            Quaternion randRot = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            );

            Vector3 shotDir = (randRot * baseDir).normalized;

            GameObject b = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.LookRotation(shotDir, Vector3.up)
            );

            EnemyBullet eb = b.GetComponent<EnemyBullet>();
            if (eb != null)
            {
                eb.Init(attackDamage, shotDir);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    void Die()
    {
        WeaponItemDrop();
        HealingItemDrop();
        Destroy(gameObject, 0.05f);
    }

    // 무기 드롭
    void WeaponItemDrop()
    {
        if (Random.value > enemySO.dropChanceSO) return;                    // Random.value = 0.0 ~ 1.0 랜덤한 값 출력
        var weaponList = enemySO.dropWeaponSO;
        if (weaponList == null || weaponList.Count == 0)
        {
            Debug.LogError("weaponList = X");
            return;
        }

        var dropWeapon = weaponList[Random.Range(0, weaponList.Count)];     // DropWeapon 랜덤으로 출력
        if (dropWeapon == null)
        {
            Debug.LogError("dropWeapon == null");
            return;
        }

        Instantiate(dropWeapon, transform.position, Quaternion.identity);   // dropWeapon 생성
    }

    // 힐 드롭
    void HealingItemDrop()
    {
        if (Random.value > enemySO.dropChanceSO) return;

        if (enemySO.healingItemSO == null)
        {
            Debug.LogError("enemySO.healingItemSO == null");
            return;
        }

        Instantiate(enemySO.healingItemSO, transform.position, Quaternion.identity);   // dropWeapon 생성
    }
}
