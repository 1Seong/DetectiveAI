using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    
    [SerializeField] private EvidenceRecorder evidenceRecorder;
    [SerializeField] private Transform bagButton;
    [SerializeField] private float bagTargetScale = 1.2f;
    [SerializeField] private Image background;
    [SerializeField] private GameObject inventoryParent;
    [SerializeField] private int maxPhoto = 10;
    private bool isInventoryOpened = false;
    private bool isInventoryOpening = false;
    
    [Header("PhotoUI")]
    [SerializeField] private Transform photoUIParent;
    [SerializeField] private GameObject photoUIPrefab;
    
    [Header("ItemUI")]
    [SerializeField] private Transform collectiveUIParent;
    [SerializeField] private GameObject collectiveUIPrefab;
    public TMP_Text nameText;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private List<PhotoData> photos =  new List<PhotoData>();
    private List<CollectiveEvidence> collectives = new List<CollectiveEvidence>();
    private void OnDestroy()
    {
        foreach (var i in photos)
        {
            if (i != null)
            {
                if (i.tex != null)
                {
                    Destroy(i.tex);
                    i.tex = null;
                }
            }
        }
    }
    
    public Vector3 GetBagButtonPos() => bagButton.position;

    [SerializeField] private int bagScaleActiveCount = 0;

    public void ScaleUpBagButton()
    {
        ++bagScaleActiveCount;
        
        // 이미 다른 아이템 때문에 확대된 상태
        if (bagScaleActiveCount > 1)
            return;
        bagButton.gameObject.SetActive(true);
        bagButton.DOKill();
        
        bagButton.DOScale(bagTargetScale, 0.3f);
    }

    public void ScaleDownBagButton()
    {
        bagScaleActiveCount = Mathf.Max(0, bagScaleActiveCount - 1);

        // 아직 연출 중인 다른 아이템이 있음
        if (bagScaleActiveCount > 0)
            return;

        bagButton.DOKill();
        
        bagButton.DOScale(1f, 0.3f);
    }

    public bool IsPhotoMax()
    {
        return photos.Count >= maxPhoto;
    }

    public void AddPhoto(PhotoData photo)
    {
        photos.Add(photo);
        // UI 프리펩 생성
        var o = Instantiate(photoUIPrefab, photoUIParent);
        o.GetComponent<PhotoUICell>().Init(photo);
    }

    public void AddCollectible(CollectiveEvidence collective)
    {
        collectives.Add(collective);
        // UI 프리펩 생성
        var o = Instantiate(collectiveUIPrefab, collectiveUIParent);
        o.GetComponent<CollectiveUICell>().Init(collective);
    }

    public void ToggleInventory()
    {
        if (isInventoryOpening) return;
        isInventoryOpening = true;
        
        if (isInventoryOpened)
        {
            isInventoryOpened = false;
            inventoryParent.SetActive(false);
            background.DOFade(0f, 0.3f).OnComplete(()=>
            {
                background.gameObject.SetActive(false);
                isInventoryOpening = false;
            });
        }
        else
        {
            isInventoryOpened = true;
            background.gameObject.SetActive(true);
            inventoryParent.SetActive(true);
            background.DOFade(250.0f / 255f, 0.3f).OnComplete(() => isInventoryOpening = false);
        }
    }

    public void DeletePhoto(int idx)
    {
        
    }
}
