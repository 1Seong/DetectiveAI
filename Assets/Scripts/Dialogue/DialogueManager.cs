using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset dialogueCsv;

    private List<DialogueData> dialogueDatas;

    private void Awake()
    {
        dialogueDatas = DialogueCsvParser.Parse(dialogueCsv);

        foreach (DialogueData data in dialogueDatas)
        {
            Debug.Log(
                $"ID: {data.ID}\n" +
                $"Speaker: {data.Speaker}\n" +
                $"Dialogue: {data.Dialogue}\n" +
                $"NextID: {data.NextID}"
            );
        }
    }
}
