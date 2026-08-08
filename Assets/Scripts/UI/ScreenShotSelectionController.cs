using System.Collections.Generic;
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
    [SerializeField] private GameObject resultGroup;
    [SerializeField] private GameObject exitButton;
    
    [SerializeField] private RectTransform dimTop;
    [SerializeField] private RectTransform dimBottom;
    [SerializeField] private RectTransform dimLeft;
    [SerializeField] private RectTransform dimRight;

    [Header("Capture")]
    [SerializeField] private GameObject[] hideWhileCapturing;

    [Header("Selection")]
    [SerializeField] private float minimumSelectionSize = 20f;
    [SerializeField] private Vector2 maxCaptureSize = new Vector2(800f, 600f);

    [Header("Animation")]
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float moveToBagDur = 0.7f;

    private RectTransform canvasRect;

    private Texture2D capturedTexture;
    private Texture2D croppedTexture;
    private List<EvidenceData> evidences = new();
    private List<string> descs = new();

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
        if (!TutorialManager.instance.isCamelTutorialCleared)
        {
            TutorialManager.instance.CamelTutorial();
            return;
        }
        
        if (InventoryManager.instance.IsPhotoMax())
        {
            // TODO : 경고 알림
            return;
        }
        
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        
        EnterScreenshotModeAsync(
            this.GetCancellationTokenOnDestroy()
        ).Forget();
    }

    private async UniTask EnterScreenshotModeAsync(
        CancellationToken cancellationToken)
    {
        //if (isCaptureMode)
            //return;

        isCaptureMode = true;
        
        screenshotRoot.SetActive(false);

        SetCaptureUIVisible(false);

        if (capturedTexture == null)
        {
            // 숨긴 UI가 렌더링 결과에 반영될 때까지 대기
            await UniTask.WaitForEndOfFrame(cancellationToken);
            ReleaseCapturedTexture();
            capturedTexture =
                ScreenCapture.CaptureScreenshotAsTexture();
        }

        ReleaseCroppedTexture();
        frozenScreen.texture = capturedTexture;

        screenshotRoot.SetActive(true);
        frozenScreen.gameObject.SetActive(true);

        selectionArea.gameObject.SetActive(false);
        dimOverlay.gameObject.SetActive(false);
        croppedImage.gameObject.SetActive(false);
        inputArea.SetActive(true);
        resultGroup.SetActive(false);
        exitButton.SetActive(true);
        ShowFullDim();

        //SetCaptureUIVisible(true);
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
        SetDimVisible(true);

        UpdateSelectionArea();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isCaptureMode || !isDragging)
            return;

        dragEnd = ClampDragEnd(eventData.position);

        UpdateSelectionArea();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isCaptureMode || !isDragging)
            return;
        
        inputArea.SetActive(false);
        isDragging = false;
        dragEnd = ClampDragEnd(eventData.position);

        Rect selectedRect = GetSelectedScreenRect();

        if (selectedRect.width < minimumSelectionSize ||
            selectedRect.height < minimumSelectionSize)
        {
            EnterScreenshotMode();
            return;
        }

        CreateAndShowResultAsync(selectedRect).Forget();
    }

    private async UniTask CreateAndShowResultAsync(
        Rect selectedRect)
    {
        CreateCroppedTexture(selectedRect);
        CaptureObjects(selectedRect);

        animationCts = new CancellationTokenSource();

        CancellationToken linkedToken =
            CancellationTokenSource.CreateLinkedTokenSource(
                animationCts.Token,
                this.GetCancellationTokenOnDestroy()
            ).Token;

        selectionArea.gameObject.SetActive(false);
        SetDimVisible(false);
        dimOverlay.gameObject.SetActive(true);
        croppedImage.gameObject.SetActive(true);
        croppedImage.DOFade(1f, 0f);

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

        croppedImage.transform.position = selectedRect.center;
        await resultRect
            .DOScale(1f, popDuration)
            .SetEase(Ease.OutBack)
            .ToUniTask(
                cancellationToken: linkedToken
            );
        await resultRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.InOutSine).ToUniTask(cancellationToken: linkedToken);
        resultGroup.SetActive(true);
    }

    private void CaptureObjects(Rect rect)
    {
        var evidenceObjects = InventoryManager.instance.GetPhotoEvidences();
        for(int i = 0; i != evidenceObjects.Length; ++i)
        {
            var o = evidenceObjects[i];
            if (o.position.x >= rect.min.x && o.position.x <= rect.max.x && o.position.y >= rect.min.y &&
                o.position.y <= rect.max.y)
            {
                evidences.Add(o.GetComponent<EvidenceObject>().data);
                descs.Add(o.GetComponent<EvidenceObject>().desc);
            }
        }
    }

    public void SaveImage()
    {
        resultGroup.SetActive(false);
        exitButton.SetActive(false);
        InventoryManager.instance.AddPhoto(PhotoDataHelper.CreatePhotoData(croppedTexture, evidences, descs));
        
        dimOverlay.gameObject.SetActive(false);
        var seq = DOTween.Sequence();
        seq.AppendCallback(InventoryManager.instance.ScaleUpBagButton);
        seq.Append(croppedImage.transform.DOMove(InventoryManager.instance.GetBagButtonPos(), moveToBagDur).SetEase(Ease.InCubic));
        seq.Join(croppedImage.transform.DOScale(0f, moveToBagDur).SetEase(Ease.InCubic).OnComplete(ExitScreenshotMode));
        seq.Join(croppedImage.DOFade(0f, moveToBagDur).SetEase(Ease.InCubic));
        seq.AppendCallback(InventoryManager.instance.ScaleDownBagButton);
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

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            selectedRect.min,
            uiCamera,
            out Vector2 localMin
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            selectedRect.max,
            uiCamera,
            out Vector2 localMax
        );

        Vector2 localCenter = (localMin + localMax) * 0.5f;
        Vector2 localSize = localMax - localMin;

        selectionArea.anchoredPosition = localCenter;
        selectionArea.sizeDelta = localSize;

        UpdateDimAreas(localMin, localMax);
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
    
    private void UpdateDimAreas(Vector2 selectionMin, Vector2 selectionMax)
    {
        Rect canvasLocalRect = canvasRect.rect;

        float canvasLeft = canvasLocalRect.xMin;
        float canvasRight = canvasLocalRect.xMax;
        float canvasBottom = canvasLocalRect.yMin;
        float canvasTop = canvasLocalRect.yMax;

        // 선택 영역 위쪽
        SetRect(
            dimTop,
            canvasLeft,
            selectionMax.y,
            canvasRight,
            canvasTop
        );

        // 선택 영역 아래쪽
        SetRect(
            dimBottom,
            canvasLeft,
            canvasBottom,
            canvasRight,
            selectionMin.y
        );

        // 선택 영역 왼쪽
        SetRect(
            dimLeft,
            canvasLeft,
            selectionMin.y,
            selectionMin.x,
            selectionMax.y
        );

        // 선택 영역 오른쪽
        SetRect(
            dimRight,
            selectionMax.x,
            selectionMin.y,
            canvasRight,
            selectionMax.y
        );
    }
    
    private void SetDimVisible(bool visible)
    {
        dimTop.gameObject.SetActive(visible);
        dimBottom.gameObject.SetActive(visible);
        dimLeft.gameObject.SetActive(visible);
        dimRight.gameObject.SetActive(visible);
    }
    
    private void ShowFullDim()
    {
        Rect rect = canvasRect.rect;

        SetRect(
            dimTop,
            rect.xMin,
            rect.yMin,
            rect.xMax,
            rect.yMax
        );

        dimBottom.sizeDelta = Vector2.zero;
        dimLeft.sizeDelta = Vector2.zero;
        dimRight.sizeDelta = Vector2.zero;

        SetDimVisible(true);
    }
    
    private void SetRect(
        RectTransform target,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        float width = Mathf.Max(0f, maxX - minX);
        float height = Mathf.Max(0f, maxY - minY);

        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = new Vector2(0.5f, 0.5f);

        target.anchoredPosition = new Vector2(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f
        );

        target.sizeDelta = new Vector2(width, height);
    }

    public void ExitScreenshotMode()
    {
        if (!isCaptureMode)
            return;

        isCaptureMode = false;
        isDragging = false;
        
        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;

        CancelCurrentAnimation();

        frozenScreen.texture = null;
        croppedImage.texture = null;

        screenshotRoot.SetActive(false);

        ReleaseCapturedTexture();
        ReleaseCroppedTexture();
        
        SetCaptureUIVisible(true);
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
    
    private Vector2 ClampDragEnd(Vector2 pointerPosition)
    {
        Vector2 screenPosition = ClampToScreen(pointerPosition);
        Vector2 offset = screenPosition - dragStart;

        offset.x = Mathf.Clamp(
            offset.x,
            -maxCaptureSize.x,
            maxCaptureSize.x
        );

        offset.y = Mathf.Clamp(
            offset.y,
            -maxCaptureSize.y,
            maxCaptureSize.y
        );

        return ClampToScreen(dragStart + offset);
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
        evidences.Clear();
        descs.Clear();
    }

    private void OnDestroy()
    {
        CancelCurrentAnimation();

        ReleaseCapturedTexture();
        ReleaseCroppedTexture();
    }
}
