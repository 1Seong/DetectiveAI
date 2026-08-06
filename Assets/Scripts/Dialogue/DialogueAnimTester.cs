using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogueAnimTester : MonoBehaviour
{
    [SerializeField]
    private DialogueTextAnimator textAnimator;

    [SerializeField] private GameObject img;

    private async UniTaskVoid Start()
    {
        List<string> dialogues = new List<string>
        {
            "여기가 사건 현장인가요?",
            "네. 하지만 저는 아무것도 보지 못했습니다.",
            "그렇다면 어젯밤에는 어디에 있었죠?",
            "아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아아"
        };

        foreach (string dialogue in dialogues)
        {
            // 출력 중 Space: 전체 표시
            // 전체 표시 후 Space: 함수 종료 및 다음 대사 진행
            await textAnimator.PlayDialogueAsync(dialogue);
        }
        
        img.SetActive(true);

        Debug.Log("모든 대사가 종료되었습니다.");
    }
}
