using UnityEngine;

public class BookButton : MonoBehaviour
{
    public void OnClick()
    {
        BookManager.Instance.OpenBook();
    }
}
