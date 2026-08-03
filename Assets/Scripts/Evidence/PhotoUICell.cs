using UnityEngine;
using UnityEngine.UI;

public class PhotoUICell : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private float zoomInMult = 2f;
    [SerializeField] private float zoomDur = 0.5f;

    private bool isZoomed = false;

    public void Init(PhotoData data)
    {
        image.texture = data.tex;
        image.SetNativeSize();
    }

    public void ZoomIn()
    {
        InventoryManager.instance.ZoomInPhoto(transform.GetSiblingIndex());
    }
}
