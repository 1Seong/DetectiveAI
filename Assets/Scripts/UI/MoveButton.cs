using UnityEngine;

public class MoveButton : MonoBehaviour
{
    [SerializeField] private GameObject fromScene;
    [SerializeField] private GameObject toScene;
    
    public async void OnClick()
    {
        InventoryManager.instance.ExitMoveMode();
        if(toScene.name == "Place0")
            AudioManager.Instance.PlayBGM(BGMType.Alley);
        if(toScene.name == "Place1")
            AudioManager.Instance.PlayBGM(BGMType.Ppuang);
        if(toScene.name == "Place4")
            AudioManager.Instance.PlayBGM(BGMType.Chocolat);
        AudioManager.Instance.StopAllSFX();
        AudioManager.Instance.PlaySFX(SFXType.Move);
        await SceneTransitionManager.Instance.FadeOutAsync();
        fromScene.SetActive(false);
        toScene.SetActive(true);
        await SceneTransitionManager.Instance.FadeInAsync();
    }
}
