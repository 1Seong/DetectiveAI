using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubmitItemUICell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image itemImg;
    private Button button;
    [SerializeField] private float targetScale = 1.2f;
    [SerializeField] private CollectiveEvidence data;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
    }

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
        if (!button.interactable) return;
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
        button.interactable = false;
        NPCManager.instance.AddEvidence(data, button, transform.position);
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
