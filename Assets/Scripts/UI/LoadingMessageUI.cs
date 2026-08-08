using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingMessageUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("문구 설정")]
    [TextArea]
    [SerializeField] private List<string> messagePool = new();

    [Min(0.1f)]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine messageCoroutine;
    private int previousIndex = -1;

    private void OnEnable()
    {
        StartMessageLoop();
    }

    private void OnDisable()
    {
        StopMessageLoop();
    }

    private void StartMessageLoop()
    {
        StopMessageLoop();

        if (messageText == null || messagePool.Count == 0)
            return;

        messageCoroutine = StartCoroutine(MessageLoop());
    }

    private void StopMessageLoop()
    {
        if (messageCoroutine == null)
            return;

        StopCoroutine(messageCoroutine);
        messageCoroutine = null;
    }

    private IEnumerator MessageLoop()
    {
        while (true)
        {
            int index = GetRandomIndex();

            messageText.text = messagePool[index];
            previousIndex = index;

            // Time.timeScale이 0이어도 문구가 교체되도록 실시간 기준 사용
            yield return new WaitForSecondsRealtime(displayDuration);
        }
    }

    private int GetRandomIndex()
    {
        if (messagePool.Count == 1)
            return 0;

        int index;

        do
        {
            index = Random.Range(0, messagePool.Count);
        }
        while (index == previousIndex);

        return index;
    }
}
