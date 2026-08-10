using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressedScale = 0.96f;

    [Header("Duration")]
    [SerializeField] private float hoverDuration = 0.15f;
    [SerializeField] private float pressDuration = 0.08f;

    [SerializeField] private Ease hoverEase = Ease.OutBack;
    [SerializeField] private Ease normalEase = Ease.OutQuad;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Tween scaleTween;

    private bool isPointerOver;
    private bool isPressed;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (!isPressed)
            ScaleTo(originalScale * hoverScale, hoverDuration, hoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (!isPressed)
            ScaleTo(originalScale, hoverDuration, normalEase);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        ScaleTo(originalScale * pressedScale, pressDuration, normalEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        Vector3 targetScale = isPointerOver
            ? originalScale * hoverScale
            : originalScale;

        ScaleTo(targetScale, pressDuration, normalEase);
    }

    private void ScaleTo(Vector3 targetScale, float duration, Ease ease)
    {
        scaleTween?.Kill();

        scaleTween = rectTransform
            .DOScale(targetScale, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        scaleTween = null;

        if (rectTransform != null)
            rectTransform.localScale = originalScale;

        isPointerOver = false;
        isPressed = false;
    }
}