using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // �÷��̾� ȭ�鿡 ��� ����
    string GetPrompt();

    void Interact(PlayerController player);
}
