using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // 플레이어 화면에 띄울 문자
    string GetPrompt();

    void Interact(PlayerController player);
}
