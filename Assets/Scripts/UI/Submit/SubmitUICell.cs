using UnityEngine;
using UnityEngine.UI;

public class SubmitUICell : MonoBehaviour
{
    private Button targetButton;
    [SerializeField] private Image image;
    [SerializeField] private RawImage rawImage;
    private CollectiveEvidence itemData;
    private PhotoData photoData;
    private SoundSource audioData;

    public void Init(CollectiveEvidence data, Button b)
    {
        itemData = data;
        image.gameObject.SetActive(true);
        image.sprite = data.sprite;
        image.SetNativeSize();
        targetButton = b;
    }
    
    public void Init(PhotoData data, Button b)
    {
        photoData = data;
        rawImage.gameObject.SetActive(true);
        rawImage.texture = data.tex;
        rawImage.SetNativeSize();
        targetButton = b;
    }

    public void Init(SoundSource data, Button b)
    {
        audioData = data;
        image.gameObject.SetActive(true);
        image.sprite = data.sprite;
        image.SetNativeSize();
        targetButton = b;
    }
    
    public void OnClick()
    {
        targetButton.interactable = true;

        if (itemData != null)
        {
            NPCManager.instance.RemoveEvidence(itemData);
        }
        else if (photoData != null)
        {
            NPCManager.instance.RemoveEvidence(photoData);
        }
        else if (audioData != null)
        {
            NPCManager.instance.RemoveEvidence(audioData);
        }
        
        Destroy(gameObject);
    }
}
