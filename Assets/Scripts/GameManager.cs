using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject optionCanvas;
    [SerializeField] private Image optionBackground;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void OpenOption()
    {
        optionCanvas.SetActive(true);
        optionBackground.DOFade(0f, 0f);
        optionBackground.DOFade(250.0f/255f, 0.3f);
    }

    public void ExitGame()
    {
        
    }
}
