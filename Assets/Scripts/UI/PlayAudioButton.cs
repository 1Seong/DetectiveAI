using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayAudioButton : MonoBehaviour
{
    [SerializeField] SFXType buttonType =  SFXType.PaperButton;
    
    private void Awake()
    {
        GetComponentInChildren<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        AudioManager.Instance.PlaySFX(buttonType);
    }
}
