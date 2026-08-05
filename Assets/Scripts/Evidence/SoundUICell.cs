using UnityEngine;
using UnityEngine.UI;

public class SoundUICell : MonoBehaviour
{
    [SerializeField] private Image image;
    private SoundSource soundSource;
    private bool isZoomed = false;

    public void Init(SoundSource data)
    {
        image.sprite = data.sprite;
        image.SetNativeSize();
        soundSource = data;
    }

    public void OnClick()
    {
        InventoryManager.instance.OpenSound(soundSource);
    }
}
