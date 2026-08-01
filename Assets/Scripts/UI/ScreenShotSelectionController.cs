using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenShotSelectionController :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private Canvas screenshotCanvas;
    [SerializeField] private GameObject screenshotRoot;
    [SerializeField] private RawImage frozenScreen;
    [SerializeField] private Image dimOverlay;
    [SerializeField] private RectTransform selectionArea;
    [SerializeField] private RawImage croppedImage;
    [SerializeField] private GameObject inputArea;

    [Header("Capture")]
    [SerializeField] private GameObject[] hideWhileCapturing;

    [Header("Selection")]
    [SerializeField] private float minimumSelectionSize = 20f;

    [Header("Animation")]
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float popDuration = 0.25f;

    private RectTransform canvasRect;

    private Texture2D capturedTexture;
    private Texture2D croppedTexture;

    private Vector2 dragStart;
    private Vector2 dragEnd;

    private bool isCaptureMode;
    private bool isDragging;

    private CancellationTokenSource animationCts;

    private void Awake()
    {
        canvasRect = screenshotCanvas.transform as RectTransform;

        screenshotRoot.SetActive(false);
        selectionArea.gameObject.SetActive(false);
        dimOverlay.gameObject.SetActive(false);
        croppedImage.gameObject.SetActive(false);
    }

    public void EnterScreenshotMode()
    {
        EnterScreenshotModeAsync(
            this.GetCancellationTokenOnDestroy()
        ).Forget();
    }

    private async UniTask EnterScreenshotModeAsync(
        CancellationToken cancellationToken)
    {
        if (isCaptureMode)
            return;

        isCaptureMode = true;

        SetCaptureUIVisible(false);

        // 숨긴 UI가 렌더링 결과에 반영될 때까지 대기
        await UniTask.WaitForEndOfFrame(cancellationToken);

        ReleaseCapturedTexture();

        capturedTexture =
            ScreenCapture.CaptureScreenshotAsTexture();

        frozenScreen.texture = capturedTexture;

        screenshotRoot.SetActive(true);
        frozenScreen.gameObject.SetActive(true);

        selectionArea.gameObject.SetActive(false);
        dimOverlay.gameObject.SetActive(false);
        croppedImage.gameObject.SetActive(false);
        inputArea.SetActive(true);

        SetCaptureUIVisible(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isCaptureMode)
            return;

        CancelCurrentAnimation();

        dragStart = ClampToScreen(eventData.position);
        dragEnd = dragStart;

        isDragging = true;

        dimOverlay.gameObject.SetActive(false);
        croppedImage.gameObject.SetActive(false);
        selectionArea.gameObject.SetActive(true);

        UpdateSelectionArea();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isCaptureMode || !isDragging)
            return;

        dragEnd = ClampToScreen(eventData.position);

        UpdateSelectionArea();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isCaptureMode || !isDragging)
            return;
        
        inputArea.SetActive(false);
        isDragging = false;
        dragEnd = ClampToScreen(eventData.position);

        Rect selectedRect = GetSelectedScreenRect();

        if (selectedRect.width < minimumSelectionSize ||
            selectedRect.height < minimumSelectionSize)
        {
            selectionArea.gameObject.SetActive(false);
            return;
        }

        CreateAndShowResultAsync(selectedRect).Forget();
    }

    private async UniTask CreateAndShowResultAsync(
        Rect selectedRect)
    {
        CreateCroppedTexture(selectedRect);

        animationCts = new CancellationTokenSource();

        CancellationToken linkedToken =
            CancellationTokenSource.CreateLinkedTokenSource(
                animationCts.Token,
                this.GetCancellationTokenOnDestroy()
            ).Token;

        selectionArea.gameObject.SetActive(false);
        dimOverlay.gameObject.SetActive(true);
        croppedImage.gameObject.SetActive(true);

        croppedImage.texture = croppedTexture;

        RectTransform resultRect = croppedImage.rectTransform;

        float scaleFactor = screenshotCanvas.scaleFactor;

        resultRect.anchoredPosition = Vector2.zero;
        resultRect.sizeDelta = new Vector2(
            croppedTexture.width / scaleFactor,
            croppedTexture.height / scaleFactor
        );

        resultRect.DOKill();

        resultRect.localScale =
            Vector3.one * startScale;

        await resultRect
            .DOScale(1f, popDuration)
            .SetEase(Ease.OutBack)
            .ToUniTask(
                cancellationToken: linkedToken
            );
    }

    private void CreateCroppedTexture(Rect selectedRect)
    {
        int x = Mathf.RoundToInt(selectedRect.xMin);
        int y = Mathf.RoundToInt(selectedRect.yMin);
        int width = Mathf.RoundToInt(selectedRect.width);
        int height = Mathf.RoundToInt(selectedRect.height);

        x = Mathf.Clamp(
            x,
            0,
            capturedTexture.width - 1
        );

        y = Mathf.Clamp(
            y,
            0,
            capturedTexture.height - 1
        );

        width = Mathf.Clamp(
            width,
            1,
            capturedTexture.width - x
        );

        height = Mathf.Clamp(
            height,
            1,
            capturedTexture.height - y
        );

        ReleaseCroppedTexture();

        croppedTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels =
            capturedTexture.GetPixels(
                x,
                y,
                width,
                height
            );

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
    }

    private void UpdateSelectionArea()
    {
        Rect selectedRect = GetSelectedScreenRect();

        Camera uiCamera =
            screenshotCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : screenshotCanvas.worldCamera;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                selectedRect.center,
                uiCamera,
                out Vector2 localCenter
            );

        float scaleFactor =
            screenshotCanvas.scaleFactor;

        selectionArea.anchoredPosition =
            localCenter;

        selectionArea.sizeDelta =
            new Vector2(
                selectedRect.width / scaleFactor,
                selectedRect.height / scaleFactor
            );
    }

    private Rect GetSelectedScreenRect()
    {
        float minX = Mathf.Min(dragStart.x, dragEnd.x);
        float minY = Mathf.Min(dragStart.y, dragEnd.y);
        float maxX = Mathf.Max(dragStart.x, dragEnd.x);
        float maxY = Mathf.Max(dragStart.y, dragEnd.y);

        return Rect.MinMaxRect(
            minX,
            minY,
            maxX,
            maxY
        );
    }

    public void ExitScreenshotMode()
    {
        if (!isCaptureMode)
            return;

        isCaptureMode = false;
        isDragging = false;

        CancelCurrentAnimation();

        frozenScreen.texture = null;
        croppedImage.texture = null;

        screenshotRoot.SetActive(false);

        ReleaseCapturedTexture();
        ReleaseCroppedTexture();
    }

    private void CancelCurrentAnimation()
    {
        animationCts?.Cancel();
        animationCts?.Dispose();
        animationCts = null;

        croppedImage.rectTransform.DOKill();
    }

    private Vector2 ClampToScreen(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(
                position.x,
                0f,
                Screen.width
            ),
            Mathf.Clamp(
                position.y,
                0f,
                Screen.height
            )
        );
    }

    private void SetCaptureUIVisible(bool visible)
    {
        foreach (GameObject target in hideWhileCapturing)
        {
            if (target != null)
                target.SetActive(visible);
        }
    }

    private void ReleaseCapturedTexture()
    {
        if (capturedTexture == null)
            return;

        Destroy(capturedTexture);
        capturedTexture = null;
    }

    private void ReleaseCroppedTexture()
    {
        if (croppedTexture == null)
            return;

        Destroy(croppedTexture);
        croppedTexture = null;
    }

    private void OnDestroy()
    {
        CancelCurrentAnimation();

        ReleaseCapturedTexture();
        ReleaseCroppedTexture();
    }
}
