using System;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance;

    [Serializable]
    public struct PageStruct
    {
        public Image image;
        public TMP_Text nameText;
        public TMP_Text descText;
    }

    [SerializeField] private Image background;
    [SerializeField] private Transform bookPanel;
    [SerializeField] private PageData[] datas;
    [SerializeField] private bool[] isUnlocked;
    [SerializeField] private PageStruct[] pageStructs;
     
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        isUnlocked = new bool[datas.Length];
        bookPanel.gameObject.SetActive(false);
    }

    public void OpenBook()
    {
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        background.gameObject.SetActive(true);
        bookPanel.gameObject.SetActive(true);
        background.DOFade(0f, 0f);
        background.DOFade(250.0f / 255f, 0.3f);
        bookPanel.DOLocalMoveY(-1200f, 0f);
        bookPanel.DOLocalMoveY(0f, 0.7f).SetEase(Ease.OutQuart);
    }

    public void CloseBook()
    {
        background.DOFade(0f, 0.5f);
        bookPanel.DOLocalMoveY(-1200f, 0.5f).SetEase(Ease.InQuart).OnComplete(() =>
        {
            background.gameObject.SetActive(false);
            bookPanel.gameObject.SetActive(false);
            GameManager.Instance.CanUseInventory = true;
            GameManager.Instance.CanUseOption = true;
        });
    }

    public void Unlock(string name)
    {
        for (int i = 4; i < datas.Length; i++)
        {
            if(datas[i].name == name)
            {
                if (isUnlocked[i]) return;
                isUnlocked[i] = true;
                pageStructs[i].image.DOColor(Color.white, 0f);
                pageStructs[i].nameText.text = name;
                pageStructs[i].descText.text = datas[i].desc;
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
