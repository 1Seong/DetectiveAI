using Cysharp.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneTransitionManager.Instance.ChangeSceneAsync("Opening").Forget();
    }

    public void Options()
    {
        GameManager.Instance.OpenOption();
    }

    public void Credits()
    {
        GameManager.Instance.OpenCredit();
    }
}
