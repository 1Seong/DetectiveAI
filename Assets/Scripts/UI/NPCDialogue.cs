using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private NPCDialogueData data;
    
    public void OnClick()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        BookManager.Instance.Unlock(data.NPCName);
        NPCManager.instance.PlayDialogue(data.Dialogues, data.sprite, data.NPCName, true).Forget();
    }
}
