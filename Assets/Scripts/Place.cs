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
    [SerializeField] private Transform photoParent;
    private void OnEnable()
    {
        Transform[] children = new Transform[photoParent.childCount];

        for (int i = 0; i < photoParent.childCount; i++)
        {
            children[i] = photoParent.GetChild(i);
        }
        
        InventoryManager.instance.SetMoveButtons(moveButton);
        InventoryManager.instance.SetSoundNonArea(soundNonArea, parrotImage, parrotText);
        InventoryManager.instance.SetPhotoEvidences(children);
    }
}
