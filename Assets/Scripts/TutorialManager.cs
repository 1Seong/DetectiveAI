using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Image moveTutorialBackground1;
    [SerializeField] private Image moveTutorialBackground2;
    [SerializeField] private Image parrotTutorialBackground;
    [SerializeField] private Image camelTutorialBackground;
    [SerializeField] private GameObject[] moveTutorial2HideObjects;
    private bool isMoveTutorialCleared = false;
    public bool isParrotTutorialCleared = false;
    public bool isCamelTutorialCleared = false;
    
    public static TutorialManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        
        var s = new List<string>()
        {
            "이곳이 사건 현장이군... 그럼 조사를 시작할까?"
        };
        await NPCManager.instance.PlayDialogueDetective(s);
        MoveTutorial1();
    }

    public void MoveTutorial1()
    {
        if (isMoveTutorialCleared) return;
        
        moveTutorialBackground1.gameObject.SetActive(true);
        moveTutorialBackground1.DOFade(250.0f / 255f, 0.3f);
    }

    public void MoveTutorial2()
    {
        if (isMoveTutorialCleared) return;
        
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(false);
        moveTutorialBackground1.gameObject.SetActive(false);
        moveTutorialBackground2.gameObject.SetActive(true);
    }

    public void ExitMoveTutorial()
    {
        if (isMoveTutorialCleared) return;
        isMoveTutorialCleared = true;
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(true);
    }

    public void ParrotTutorial()
    {
        if (isParrotTutorialCleared) return;
        
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(false);
        isParrotTutorialCleared = true;
        parrotTutorialBackground.gameObject.SetActive(true);
        parrotTutorialBackground.DOFade(250.0f / 255f, 0.3f);
    }

    public void ExitParrotTutorial()
    {
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(true);
        parrotTutorialBackground.gameObject.SetActive(false);
    }

    public void CamelTutorial()
    {
        if (isCamelTutorialCleared) return;
        
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(false);
        isCamelTutorialCleared = true;
        camelTutorialBackground.gameObject.SetActive(true);
        camelTutorialBackground.DOFade(250.0f / 255f, 0.3f);
    }
    
    public void ExitCamelTutorial()
    {
        foreach(var o in moveTutorial2HideObjects)
            o.SetActive(true);
        camelTutorialBackground.gameObject.SetActive(false);
    }
}
