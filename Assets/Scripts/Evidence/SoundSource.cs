using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SoundSource : MonoBehaviour
{
    public Sprite sprite;
    public AudioClip clip;
    public EvidenceData data;

    private bool isCollected = false;

    public void OnClick()
    {
        if (isCollected) return;
        
        InventoryManager.instance.AddSound(this);
        isCollected = true;
        GetComponent<Button>().enabled = false;
    }
}
