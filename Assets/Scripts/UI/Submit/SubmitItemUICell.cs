using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubmitItemUICell : MonoBehaviour
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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemImg.transform.DOScale(1f, 0.2f);
        InventoryManager.instance.nameText.text = "";
    }

    public void OnClick()
    {
        var b = GetComponentInChildren<Button>();
        b.interactable = false;
        NPCManager.instance.AddEvidence(data, b, transform.position);
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
