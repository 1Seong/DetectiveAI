using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Place : MonoBehaviour
{
    [SerializeField] private GameObject moveButton;
    [SerializeField] private GameObject soundNonArea;
    [SerializeField] private Image parrotImage;
    [SerializeField] private TMP_Text parrotText;
    private void OnEnable()
    {
        InventoryManager.instance.SetMoveButtons(moveButton);
        InventoryManager.instance.SetSoundNonArea(soundNonArea, parrotImage, parrotText);
    }
}
