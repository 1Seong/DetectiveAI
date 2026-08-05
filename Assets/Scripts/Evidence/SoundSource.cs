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
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        InventoryManager.instance.AddSoundButton(button);
    }

    private void OnDisable()
    {
        InventoryManager.instance.DeleteSoundButton(button);
    }

    public void OnClick()
    {
        if (isCollected) return;
        
        InventoryManager.instance.AddSound(this);
        isCollected = true;
        GetComponent<Button>().enabled = false;
    }
}
