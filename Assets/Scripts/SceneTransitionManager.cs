using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade UI")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.5f;
    [SerializeField] private Ease fadeOutEase = Ease.OutQuad;
    [SerializeField] private Ease fadeInEase = Ease.InQuad;

    [Tooltip("게임 시작 시 검은 화면에서 서서히 밝아집니다.")]
    [SerializeField] private bool fadeInOnStart = true;

    public bool IsTransitioning { get; private set; }

    private Tween currentFadeTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("Fade CanvasGroup이 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        fadeCanvasGroup.alpha = fadeInOnStart ? 1f : 0f;
        fadeCanvasGroup.blocksRaycasts = fadeInOnStart;
        fadeCanvasGroup.interactable = false;
    }

    private void Start()
    {
        if (fadeInOnStart)
            FadeInAsync(destroyCancellationToken).Forget();
    }

    /// <summary>
    /// 씬 이름으로 전환합니다.
    /// </summary>
    public async UniTask ChangeSceneAsync(
        string sceneName,
        CancellationToken cancellationToken = default)
    {
        if (IsTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("전환할 씬 이름이 비어 있습니다.");
            return;
        }

        IsTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        try
        {
            await FadeOutAsync(cancellationToken);

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            if (operation == null)
                throw new InvalidOperationException(
                    $"씬을 불러올 수 없습니다: {sceneName}");

            await operation.ToUniTask(cancellationToken: cancellationToken);

            // 새 씬의 첫 프레임 초기화 이후 암전을 해제합니다.
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate,
                cancellationToken);

            await FadeInAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 오브젝트 파괴 또는 외부 취소 시 별도 오류를 출력하지 않습니다.
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            // 로드 실패 시 검은 화면에 갇히지 않도록 복구합니다.
            await FadeInAsync(destroyCancellationToken);
        }
        finally
        {
            fadeCanvasGroup.blocksRaycasts = false;
            IsTransitioning = false;
        }
    }

    /// <summary>
    /// 빌드 인덱스로 씬을 전환합니다.
    /// </summary>
    public async UniTask ChangeSceneAsync(
        int buildIndex,
        CancellationToken cancellationToken = default)
    {
        if (IsTransitioning)
            return;

        if (buildIndex < 0 ||
            buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"유효하지 않은 씬 빌드 인덱스입니다: {buildIndex}");
            return;
        }

        IsTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        try
        {
            await FadeOutAsync(cancellationToken);

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);

            if (operation == null)
                throw new InvalidOperationException(
                    $"씬을 불러올 수 없습니다: {buildIndex}");

            await operation.ToUniTask(cancellationToken: cancellationToken);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate,
                cancellationToken);

            await FadeInAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            await FadeInAsync(destroyCancellationToken);
        }
        finally
        {
            fadeCanvasGroup.blocksRaycasts = false;
            IsTransitioning = false;
        }
    }

    public UniTask FadeOutAsync(
        CancellationToken cancellationToken = default)
    {
        fadeCanvasGroup.blocksRaycasts = true;

        return PlayFadeAsync(
            targetAlpha: 1f,
            duration: fadeOutDuration,
            ease: fadeOutEase,
            cancellationToken
        );
    }

    public async UniTask FadeInAsync(
        CancellationToken cancellationToken = default)
    {
        await PlayFadeAsync(
            targetAlpha: 0f,
            duration: fadeInDuration,
            ease: fadeInEase,
            cancellationToken
        );

        fadeCanvasGroup.blocksRaycasts = false;
    }

    private async UniTask PlayFadeAsync(
        float targetAlpha,
        float duration,
        Ease ease,
        CancellationToken cancellationToken)
    {
        currentFadeTween?.Kill();

        if (duration <= 0f)
        {
            fadeCanvasGroup.alpha = targetAlpha;
            return;
        }

        currentFadeTween = fadeCanvasGroup
            .DOFade(targetAlpha, duration)
            .SetEase(ease)
            .SetUpdate(true); // Time.timeScale이 0이어도 실행

        await currentFadeTween.ToUniTask(
            cancellationToken: cancellationToken
        );

        currentFadeTween = null;
    }

    private void OnDestroy()
    {
        currentFadeTween?.Kill();

        if (Instance == this)
            Instance = null;
    }
}