using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType { Spray, RapidFire, SingleShot }        // 난사, 연사, 단발
public enum EnemyMoveType { MoveTarget, MoveT, MoveF }              // 플레이어를 향해, 기준 없이 이동, 움직임 X

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
    [Range(0f, 1f)] public float dropChanceSO;      // [Range(0f, 1f)] <- 인스펙터 창에서 입력을 0.0 ~ 1.0으로 제한 + 슬라이더 추가
    public GameObject healingItemSO;
    public List<GameObject> dropWeaponSO;
}
