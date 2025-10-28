using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType { Spray, RapidFire, SingleShot }        // ����, ����, �ܹ�
public enum EnemyMoveType { MoveTarget, MoveT, MoveF }              // �÷��̾ ����, ���� ���� �̵�, ������ X

[CreateAssetMenu(menuName = "Game/Enemy", fileName = "EnemyStat")]
public class EnemySO : ScriptableObject
{
    [Header("Attack")]
    public EnemyAttackType enemyAttackType;
    public GameObject bulletPrefab;
    public int attackDamageSO;
    public float attackSpeedSO;
    public float attackCooldownSO;
    public float attackRangeSO;

    [Header("Rapid")]
    public float rapidTimerSO;

    [Header("Stat")]
    public int hpSO;
    public EnemyMoveType enemyMoveType;
    public float moveSpeedSO;

    [Header("Drop")]
    [Range(0f, 1f)] public float dropChanceSO;      // [Range(0f, 1f)] <- �ν����� â���� �Է��� 0.0 ~ 1.0���� ���� + �����̴� �߰�
    public GameObject healingItemSO;
    public List<GameObject> dropWeaponSO;
}
