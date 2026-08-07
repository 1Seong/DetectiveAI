using UnityEngine;

public class OptionButton : MonoBehaviour
{
    public void OnClick()
    {
        GameManager.Instance.OpenOption();
    }
}
