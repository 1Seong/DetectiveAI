using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void OnClick()
    {
        GameManager.Instance.ReturnToTitle();
    }
}
