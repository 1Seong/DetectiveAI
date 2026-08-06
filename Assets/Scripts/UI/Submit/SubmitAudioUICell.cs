using UnityEngine;
using UnityEngine.UI;

public class SubmitAudioUICell : MonoBehaviour
{
    [SerializeField] private Image image;
    private SoundSource soundSource;

    public void Init(SoundSource data)
    {
        image.sprite = data.sprite;
        image.SetNativeSize();
        soundSource = data;
    }

    public void OnClick()
    {
        var b = GetComponentInChildren<Button>();
        b.interactable = false;
        NPCManager.instance.AddEvidence(soundSource, b, transform.position);
    }
}
