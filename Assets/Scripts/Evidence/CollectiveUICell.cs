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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemImg.transform.DOScale(targetScale, 0.3f);
        InventoryManager.instance.nameText.gameObject.SetActive(true);
        InventoryManager.instance.nameText.text = data.data.evidenceId;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemImg.transform.DOScale(1f, 0.3f);
        InventoryManager.instance.nameText.gameObject.SetActive(false);
        InventoryManager.instance.nameText.text = "";
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
