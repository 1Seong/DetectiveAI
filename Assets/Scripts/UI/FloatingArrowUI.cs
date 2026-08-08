using DG.Tweening;
using UnityEngine;

public class FloatingArrowUI : MonoBehaviour
{
    [Header("이동 대상")]
    [SerializeField] private RectTransform target;

    [Header("흔들림 설정")]
    [SerializeField] private float moveDistance = 12f;
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool moveBackwards = false;

    private Tween floatingTween;
    private float originalY;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalY = target.anchoredPosition.y;
    }

    private void OnEnable()
    {
        StartFloating();
    }

    private void OnDisable()
    {
        StopFloating();
    }

    public void StartFloating()
    {
        if (target == null)
            return;

        floatingTween?.Kill();

        Vector2 position = target.anchoredPosition;
        position.y = originalY;
        target.anchoredPosition = position;

        floatingTween = target
            .DOAnchorPosY(moveBackwards ? originalY - moveDistance : originalY + moveDistance, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(useUnscaledTime);
    }

    public void StopFloating()
    {
        floatingTween?.Kill();
        floatingTween = null;

        if (target == null)
            return;

        Vector2 position = target.anchoredPosition;
        position.y = originalY;
        target.anchoredPosition = position;
    }

    private void OnDestroy()
    {
        floatingTween?.Kill();
    }
}
