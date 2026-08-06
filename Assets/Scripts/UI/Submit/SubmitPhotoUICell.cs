using UnityEngine;
using UnityEngine.UI;

public class SubmitPhotoUICell : MonoBehaviour
{
    [SerializeField] private RawImage image;
    private PhotoData data;

    public void Init(PhotoData data)
    {
        image.texture = data.tex;
        image.SetNativeSize();
        this.data = data;
    }

    public void OnClick()
    {
        var b = GetComponentInChildren<Button>();
        b.interactable = false;
        NPCManager.instance.AddEvidence(data, b, transform.position);
    }
}
