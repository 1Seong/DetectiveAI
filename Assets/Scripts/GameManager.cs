using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Image optionBackground;
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button exitButton;

    public bool CanUseOption;
    public bool CanUseInventory;
    private bool isOpened = false;
    
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CanUseOption)
        {
            if (isOpened)
            {
                CloseOption();
            }
            else
            {
                OpenOption();
            }
        }
    }

    public void OpenOption()
    {
        isOpened = true;
        CanUseInventory = false;
        optionPanel.SetActive(true);
        optionBackground.gameObject.SetActive(true);
        creditButton.interactable = true;
        exitButton.interactable = true;
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            creditButton.interactable = false;
            exitButton.interactable = false;
        }
        optionBackground.DOFade(0f, 0f);
        optionBackground.DOFade(250.0f/255f, 0.3f);
    }

    public void CloseOption()
    {
        isOpened = false;
        CanUseInventory = true;
        optionPanel.SetActive(false);
        optionBackground.gameObject.SetActive(false);
    }

    public void ReturnToTitle()
    {
        CloseOption();
        SceneTransitionManager.Instance.ChangeSceneAsync("MainMenu").Forget();
    }

    public void OpenCredit()
    {
        creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        creditPanel.SetActive(false);
    }
}
