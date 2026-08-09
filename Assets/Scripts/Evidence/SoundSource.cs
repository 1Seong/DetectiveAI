using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SoundSource : MonoBehaviour
{
    public Sprite sprite;
    public SFXType type;
    public bool isLoop = false;
    public float loopCycle = 0f;
    public EvidenceData data;
    [TextArea]
    public string desc;

    private bool isCollected = false;
    private Button button;
    private float sfxTime = 0;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        InventoryManager.instance.AddSoundButton(button);
        AudioManager.Instance.PlaySFX(type);
        sfxTime = 0;
    }

    private void Update()
    {
        if (isLoop)
        {
            sfxTime += Time.deltaTime;
            if (sfxTime > loopCycle)
            {
                AudioManager.Instance.PlaySFX(type);
                sfxTime = 0;
            }
        }
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
