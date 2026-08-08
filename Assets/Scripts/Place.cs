using System;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField] private GameObject moveButton;
    private void OnEnable()
    {
        InventoryManager.instance.SetMoveButtons(moveButton);
    }
}
