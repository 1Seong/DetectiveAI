using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextWaveAnimator : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private float amplitude = 8f;
    [SerializeField] private float frequency = 6f;
    [SerializeField] private float characterInterval = 0.7f;
    [SerializeField] private bool useUnscaledTime = true;

    private TMP_Text targetText;
    private CancellationTokenSource animationCts;

    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
    }

    public void Play(string text)
    {
        Stop();

        targetText.text = text;
        animationCts = new CancellationTokenSource();

        AnimateAsync(animationCts.Token).Forget();
    }

    public void Stop()
    {
        if (animationCts != null)
        {
            animationCts.Cancel();
            animationCts.Dispose();
            animationCts = null;
        }

        // 변형된 글자 메시를 원래 상태로 복구
        if (targetText != null)
        {
            targetText.ForceMeshUpdate();
        }
    }

    private async UniTaskVoid AnimateAsync(CancellationToken token)
    {
        targetText.ForceMeshUpdate();

        TMP_TextInfo textInfo = targetText.textInfo;

        // 원본 정점 좌표를 보관
        Vector3[][] originalVertices =
            new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] =
                (Vector3[])textInfo.meshInfo[i].vertices.Clone();
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                float time = useUnscaledTime
                    ? Time.unscaledTime
                    : Time.time;

                // 원본 정점에서 매 프레임 다시 계산
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    originalVertices[i].CopyTo(
                        textInfo.meshInfo[i].vertices,
                        0
                    );
                }

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo characterInfo =
                        textInfo.characterInfo[i];

                    // 공백, 줄바꿈 등 화면에 보이지 않는 문자는 제외
                    if (!characterInfo.isVisible)
                        continue;

                    int materialIndex = characterInfo.materialReferenceIndex;
                    int vertexIndex = characterInfo.vertexIndex;

                    Vector3[] vertices =
                        textInfo.meshInfo[materialIndex].vertices;

                    float wave =
                        Mathf.Sin(
                            time * frequency -
                            i * characterInterval
                        ) * amplitude;

                    Vector3 offset = Vector3.up * wave;

                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;
                }

                // 변경된 정점을 TMP 메시로 반영
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

                    meshInfo.mesh.vertices = meshInfo.vertices;

                    targetText.UpdateGeometry(
                        meshInfo.mesh,
                        i
                    );
                }

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    token
                );
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 애니메이션 종료
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDestroy()
    {
        Stop();
    }
}
