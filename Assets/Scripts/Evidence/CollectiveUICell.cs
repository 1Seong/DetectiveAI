using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectiveUICell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image itemImg;
    [SerializeField] private float targetScale = 1.2f;
    [SerializeField] private CollectiveEvidence data;
    
    public void Init(CollectiveEvidence data)
    {
        var image = GetComponentOnlyInChildren<Image>();
        image.sprite = data.sprite;
        image.SetNativeSize();
        itemImg = image;
        this.data = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemImg.transform.DOScale(targetScale, 0.2f);
        InventoryManager.instance.nameText.text = data.data.evidenceId;
        InventoryManager.instance.itemDescText.text = data.desc;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemImg.transform.DOScale(1f, 0.2f);
        InventoryManager.instance.nameText.text = "";
        InventoryManager.instance.itemDescText.text = "";
    }
    
    T GetComponentOnlyInChildren<T>() where T : Component
    {
        foreach (Transform child in transform)
        {
            T component = child.GetComponentInChildren<T>(true);

            if (component != null)
                return component;
        }

        return null;
    }
}
