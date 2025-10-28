using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, IInteractable
{
    public enum PickupType { Weapon, Heal }
    public PickupType pickupType;

    [Header("Weapon Pickup")]
    public Weapon weaponPrefab;     // �÷��̾ ������ ����
    public string weaponDisplayName;      // UI�� ������ �̸�
    public Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);  // ���⸦ �� ���� �󸶴� ���ų�

    [Header("Heal Pickup")]
    public int healAmount = 20;     // ü�� ȸ����
    public string healDisplayName = "Heal";

    [Header("UI")]
    public string weaponUI = "[F] ���� ȹ��: {0}";
    public string healUI = "[F] ü�� ȸ�� +{0}";

    public string GetPrompt()
    {
        switch (pickupType)
        {
            case PickupType.Weapon:
                string shownName = string.IsNullOrEmpty(weaponDisplayName) ? (weaponPrefab != null ? weaponPrefab.name : "???") : weaponDisplayName;
                return string.Format(weaponUI, shownName);

            case PickupType.Heal:
                return string.Format(healUI, healAmount);

            default:
                return "[F] ��ȣ�ۿ�";
        }
    }

    public void Interact(PlayerController player)
    {
        if (!player) return;

        switch (pickupType)
        {
            case PickupType.Weapon:
                InteractWeapon(player);
                break;

            case PickupType.Heal:
                InteractHeal(player);
                break;
        }
    }

    void InteractWeapon(PlayerController player)
    {
        if (weaponPrefab == null) return;

        // ���� ���� ���
        if (player.currentWeapon != null)
        {
            Weapon oldWeapon = player.currentWeapon;

            if (oldWeapon.worldPickupPrefab != null)
            {
                Vector3 dropPos = player.transform.position + dropOffset;
                Instantiate(oldWeapon.worldPickupPrefab, dropPos, Quaternion.identity);
            }

            Object.Destroy(oldWeapon.gameObject);
            player.currentWeapon = null;
        }

        player.EquipWeapon(weaponPrefab);

        Destroy(gameObject);
    }

    void InteractHeal(PlayerController player)
    {
        if (player.currentHP >= player.maxHP) return;

        player.Heal(healAmount);

        Destroy(gameObject);
    }
}
