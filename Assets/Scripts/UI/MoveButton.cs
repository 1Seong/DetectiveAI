using UnityEngine;

public class MoveButton : MonoBehaviour
{
    [SerializeField] private GameObject fromScene;
    [SerializeField] private GameObject toScene;
    
    public async void OnClick()
    {
        InventoryManager.instance.ExitMoveMode();
        await SceneTransitionManager.Instance.FadeOutAsync();
        fromScene.SetActive(false);
        toScene.SetActive(true);
        await SceneTransitionManager.Instance.FadeInAsync();
    }
}
