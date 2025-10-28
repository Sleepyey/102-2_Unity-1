using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;     // Singleton
    [Header("Game Data")]
    public int currentStage = 1;

    [Header("Player Data")]
    public int playerCurrentHP = 100;

    // Awake
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���� ��������
    public void SetStage(int stage)
    {
        currentStage = stage;
    }

    // ���� ü��
    public void InitPlayerHP(int currentHP)
    {
        playerCurrentHP = currentHP;
    }
}
