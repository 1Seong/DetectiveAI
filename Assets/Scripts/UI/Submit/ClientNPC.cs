using UnityEngine;

public class ClientNPC : MonoBehaviour
{
    public void OnClick()
    {
        NPCManager.instance.AskDeduction();
    }
}
