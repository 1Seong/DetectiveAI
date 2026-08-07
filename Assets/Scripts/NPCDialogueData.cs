using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogueData", menuName = "Scriptable Objects/NPCDialogueData")]
public class NPCDialogueData : ScriptableObject
{
    public Sprite sprite;
    public string NPCName;
    public List<string> Dialogues;
}
