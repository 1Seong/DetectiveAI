using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogueTextAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TMP_Text dialogueText;

    [Header("Typing")]
    [SerializeField, Min(0f)]
    private float characterInterval = 0.07f;

    [Header("Character Animation")]
    [SerializeField, Min(0f)]
    private float animationDuration = 0.4f;

    [SerializeField]
    private float startYOffset = -20f;

    [SerializeField]
    private Ease moveEase = Ease.OutBack;

    private CancellationToken destroyCancellationToken;

    private bool isRevealing;
    private bool skipRequested;

    private readonly object tweenTarget = new object();

    private void Awake()
    {
        destroyCancellationToken = this.GetCancellationTokenOnDestroy();

        if (dialogueText == null)
            dialogueText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (isRevealing)
            skipRequested = true;
    }

    /// <summary>
    /// 대사를 출력하고, 전체 출력 이후 다음 Space 입력까지 기다립니다.
    /// 함수가 종료되면 다음 대사를 출력해도 됩니다.
    /// </summary>
    public async UniTask PlayDialogueAsync(
        string dialogue,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken,
                cancellationToken
            );

        CancellationToken token = linkedCts.Token;

        await RevealTextAsync(dialogue, token);

        // 전체 출력과 동시에 눌린 입력이 다음 대사 넘김으로 사용되지 않게 함
        await UniTask.Yield(PlayerLoopTiming.Update, token);

        // 전체 대사가 표시된 상태에서 다음 스페이스 입력 대기
        await UniTask.WaitUntil(
            () => Input.GetKeyDown(KeyCode.Space),
            cancellationToken: token
        );

        // 방금 누른 스페이스가 완전히 해제될 때까지 대기
        await UniTask.WaitUntil(
            () => !Input.GetKey(KeyCode.Space),
            cancellationToken: token
        );

        // 입력 해제 프레임까지 다음 대사로 전달되지 않도록 한 프레임 추가 대기
        await UniTask.Yield(PlayerLoopTiming.Update, token);
    }

    /// <summary>
    /// 다음 입력을 기다리지 않고 글자 출력만 실행합니다.
    /// </summary>
    public async UniTask RevealTextAsync(
        string dialogue,
        CancellationToken cancellationToken = default)
    {
        DOTween.Kill(tweenTarget);

        skipRequested = false;
        isRevealing = true;

        dialogueText.text = dialogue ?? string.Empty;

        // TextMeshPro가 문자별 정점 정보를 즉시 생성하도록 합니다.
        dialogueText.ForceMeshUpdate();

        TMP_TextInfo textInfo = dialogueText.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0)
        {
            isRevealing = false;
            return;
        }

        Vector3[][] originalVertices = CopyVertices(textInfo);
        Color32[][] originalColors = CopyColors(textInfo);

        HideAllCharacters(textInfo, originalVertices, originalColors);
        dialogueText.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices |
            TMP_VertexDataUpdateFlags.Colors32
        );

        try
        {
            for (int characterIndex = 0;
                 characterIndex < characterCount;
                 characterIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skipRequested)
                {
                    ShowAllCharacters(
                        textInfo,
                        originalVertices,
                        originalColors
                    );

                    dialogueText.UpdateVertexData(
                        TMP_VertexDataUpdateFlags.Vertices |
                        TMP_VertexDataUpdateFlags.Colors32
                    );

                    return;
                }

                TMP_CharacterInfo characterInfo =
                    textInfo.characterInfo[characterIndex];

                // 공백이나 줄바꿈은 표시할 정점이 없습니다.
                if (!characterInfo.isVisible)
                    continue;

                AnimateCharacter(
                    characterIndex,
                    textInfo,
                    originalVertices,
                    originalColors
                );

                if (characterInterval > 0f)
                {
                    await WaitCharacterIntervalAsync(
                        characterInterval,
                        cancellationToken
                    );
                }
            }

            // 마지막 글자의 애니메이션이 끝날 때까지 기다립니다.
            float remainingDuration =
                Mathf.Max(animationDuration - characterInterval, 0f);

            if (remainingDuration > 0f && !skipRequested)
            {
                await WaitCharacterIntervalAsync(
                    remainingDuration,
                    cancellationToken
                );
            }

            if (skipRequested)
            {
                DOTween.Kill(tweenTarget);

                ShowAllCharacters(
                    textInfo,
                    originalVertices,
                    originalColors
                );

                dialogueText.UpdateVertexData(
                    TMP_VertexDataUpdateFlags.Vertices |
                    TMP_VertexDataUpdateFlags.Colors32
                );
            }
        }
        finally
        {
            isRevealing = false;
        }
    }

    private void AnimateCharacter(
        int characterIndex,
        TMP_TextInfo textInfo,
        Vector3[][] originalVertices,
        Color32[][] originalColors)
    {
        TMP_CharacterInfo characterInfo =
            textInfo.characterInfo[characterIndex];

        int materialIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;

        float progress = 0f;

        DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;

                    ApplyCharacterProgress(
                        characterIndex,
                        progress,
                        textInfo,
                        originalVertices,
                        originalColors
                    );
                },
                1f,
                animationDuration
            )
            .SetEase(moveEase)
            .SetTarget(tweenTarget)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                SetCharacterToOriginal(
                    materialIndex,
                    vertexIndex,
                    textInfo,
                    originalVertices,
                    originalColors
                );

                dialogueText.UpdateVertexData(
                    TMP_VertexDataUpdateFlags.Vertices |
                    TMP_VertexDataUpdateFlags.Colors32
                );
            });
    }

    private void ApplyCharacterProgress(
        int characterIndex,
        float progress,
        TMP_TextInfo textInfo,
        Vector3[][] originalVertices,
        Color32[][] originalColors)
    {
        TMP_CharacterInfo characterInfo =
            textInfo.characterInfo[characterIndex];

        if (!characterInfo.isVisible)
            return;

        int materialIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;

        Vector3[] vertices =
            textInfo.meshInfo[materialIndex].vertices;

        Color32[] colors =
            textInfo.meshInfo[materialIndex].colors32;

        Vector3 offset = Vector3.up *
            Mathf.Lerp(startYOffset, 0f, progress);

        byte alpha = (byte)Mathf.RoundToInt(
            Mathf.Lerp(0f, 255f, progress)
        );

        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i] =
                originalVertices[materialIndex][vertexIndex + i]
                + offset;

            Color32 originalColor =
                originalColors[materialIndex][vertexIndex + i];

            colors[vertexIndex + i] = new Color32(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                (byte)(
                    originalColor.a *
                    alpha / 255
                )
            );
        }

        dialogueText.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices |
            TMP_VertexDataUpdateFlags.Colors32
        );
    }

    private async UniTask WaitCharacterIntervalAsync(
        float duration,
        CancellationToken cancellationToken)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (skipRequested)
                return;

            elapsedTime += Time.deltaTime;

            await UniTask.Yield(
                PlayerLoopTiming.Update,
                cancellationToken
            );
        }
    }

    private void HideAllCharacters(
        TMP_TextInfo textInfo,
        Vector3[][] originalVertices,
        Color32[][] originalColors)
    {
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo =
                textInfo.characterInfo[i];

            if (!characterInfo.isVisible)
                continue;

            int materialIndex =
                characterInfo.materialReferenceIndex;

            int vertexIndex =
                characterInfo.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            Color32[] colors =
                textInfo.meshInfo[materialIndex].colors32;

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] =
                    originalVertices[materialIndex][vertexIndex + j]
                    + Vector3.up * startYOffset;

                Color32 originalColor =
                    originalColors[materialIndex][vertexIndex + j];

                colors[vertexIndex + j] = new Color32(
                    originalColor.r,
                    originalColor.g,
                    originalColor.b,
                    0
                );
            }
        }
    }

    private void ShowAllCharacters(
        TMP_TextInfo textInfo,
        Vector3[][] originalVertices,
        Color32[][] originalColors)
    {
        DOTween.Kill(tweenTarget);

        for (int materialIndex = 0;
             materialIndex < textInfo.meshInfo.Length;
             materialIndex++)
        {
            Array.Copy(
                originalVertices[materialIndex],
                textInfo.meshInfo[materialIndex].vertices,
                originalVertices[materialIndex].Length
            );

            Array.Copy(
                originalColors[materialIndex],
                textInfo.meshInfo[materialIndex].colors32,
                originalColors[materialIndex].Length
            );
        }
    }

    private void SetCharacterToOriginal(
        int materialIndex,
        int vertexIndex,
        TMP_TextInfo textInfo,
        Vector3[][] originalVertices,
        Color32[][] originalColors)
    {
        Vector3[] vertices =
            textInfo.meshInfo[materialIndex].vertices;

        Color32[] colors =
            textInfo.meshInfo[materialIndex].colors32;

        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i] =
                originalVertices[materialIndex][vertexIndex + i];

            colors[vertexIndex + i] =
                originalColors[materialIndex][vertexIndex + i];
        }
    }

    private static Vector3[][] CopyVertices(
        TMP_TextInfo textInfo)
    {
        Vector3[][] copiedVertices =
            new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] source =
                textInfo.meshInfo[i].vertices;

            copiedVertices[i] =
                new Vector3[source.Length];

            Array.Copy(
                source,
                copiedVertices[i],
                source.Length
            );
        }

        return copiedVertices;
    }

    private static Color32[][] CopyColors(
        TMP_TextInfo textInfo)
    {
        Color32[][] copiedColors =
            new Color32[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Color32[] source =
                textInfo.meshInfo[i].colors32;

            copiedColors[i] =
                new Color32[source.Length];

            Array.Copy(
                source,
                copiedColors[i],
                source.Length
            );
        }

        return copiedColors;
    }

    private void OnDisable()
    {
        DOTween.Kill(tweenTarget);

        isRevealing = false;
        skipRequested = false;
    }
}
