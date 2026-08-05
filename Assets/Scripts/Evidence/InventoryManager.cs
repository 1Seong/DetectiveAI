using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private GameObject zoomPanel;
    [SerializeField] private Image zoomBackground;
    [SerializeField] private RawImage zoomRawImg;
    [SerializeField] private GameObject[] zoomPanelButtons;
    [SerializeField] private float dissolveDuration = 0.4f;
    
    [Header("ItemUI")]
    [SerializeField] private Transform collectiveUIParent;
    [SerializeField] private GameObject collectiveUIPrefab;
    public TMP_Text nameText;
    
    [Header("SoundUI")]
    [SerializeField] private Transform soundUIParent;
    [SerializeField] private GameObject soundUIPrefab;
    [SerializeField] private SoundController soundController;
    [SerializeField] private GameObject[] hideObjects;
    [SerializeField] private GameObject exitSoundButton;
    [SerializeField] private GameObject soundNoneArea;
    [SerializeField] private TMP_Text parrotText;
    [SerializeField] private Image parrotImg;
    [SerializeField] private Sprite normalParrot;
    [SerializeField] private Sprite sadParrot;
    [SerializeField] private Transform soundObjectParent;
    private Button[] soundObjects;
    
    void Awake()
    {
        if (instance == null)
            instance = this;

        soundObjects = soundObjectParent.GetComponentsInChildren<Button>();
    }

    [SerializeField] private List<PhotoData> photos =  new List<PhotoData>();
    [SerializeField] private List<CollectiveEvidence> collectives = new List<CollectiveEvidence>();
    [SerializeField] private List<SoundSource> soundSources = new List<SoundSource>();
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
    private int currentPhotoIdx = 0;

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

    public void ZoomInPhoto(int idx)
    {
        currentPhotoIdx = idx;
        zoomRawImg.texture = photos[idx].tex;
        zoomRawImg.SetNativeSize();
        foreach (var i in zoomPanelButtons)
            i.SetActive(true);
        zoomPanel.SetActive(true);
        zoomRawImg.transform.DOScale(0.85f, 0f);
        zoomRawImg.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        zoomBackground.DOFade(0f, 0f);
        zoomBackground.DOFade(250.0f / 255f, 0.3f);
    }

    public void ZoomOutPhoto()
    {
        zoomPanel.SetActive(false);
    }
    
    public void DeletePhoto()
    {
        DeletePhotoAsync().Forget();
    }

    public async UniTaskVoid DeletePhotoAsync()
    {
        int DissolveAmountId = Shader.PropertyToID("_Progress");
        int RectSizeId = Shader.PropertyToID("_RectSize");
        var dissolveMaterial = zoomRawImg.material;
        var rect = zoomRawImg.rectTransform.rect;
        dissolveMaterial.SetFloat(DissolveAmountId, 0f);
        dissolveMaterial.SetVector(
            RectSizeId,
            new Vector4(
                rect.width,
                rect.height,
                0f,
                0f
            )
        );
        foreach (var i in zoomPanelButtons)
            i.SetActive(false);
        await DOTween.To(
            () => dissolveMaterial.GetFloat(DissolveAmountId),
            value => dissolveMaterial.SetFloat(DissolveAmountId, value),
            1.05f,
            dissolveDuration
        ).ToUniTask();
        await zoomBackground.DOFade(0f, 0.3f).ToUniTask();
        
        int idx = currentPhotoIdx;
        zoomPanel.SetActive(false);
        if (photos[idx].tex != null)
        {
            Destroy(photos[idx].tex);
            photos[idx].tex = null;
        }
        photos.RemoveAt(idx);
        Destroy(photoUIParent.transform.GetChild(idx).gameObject);
        
        dissolveMaterial.SetFloat(DissolveAmountId, 0f);
    }

    public void AddCollectible(CollectiveEvidence collective)
    {
        collectives.Add(collective);
        // UI 프리펩 생성
        var o = Instantiate(collectiveUIPrefab, collectiveUIParent);
        o.GetComponent<CollectiveUICell>().Init(collective);
    }

    public void EnterSoundMode()
    {
        parrotText.text = "기억할 소리를 선택하세요";
        parrotImg.sprite = normalParrot;
        soundNoneArea.SetActive(true);
        exitSoundButton.SetActive(true);
        foreach(var o in hideObjects)
            o.SetActive(false);
        foreach (var b in soundObjects)
            b.enabled = true;
    }

    public void ExitSoundMode()
    {
        soundNoneArea.SetActive(false);
        exitSoundButton.SetActive(false);
        foreach(var o in hideObjects)
            o.SetActive(true);
        foreach (var b in soundObjects)
            b.enabled = false;
    }

    public void AddSound(SoundSource sound)
    {
        soundSources.Add(sound);
        
        var o = Instantiate(soundUIPrefab, soundUIParent);
        o.GetComponent<SoundUICell>().Init(sound);
        soundController.CollectSound(sound);
        parrotText.text = "기억할 소리를 선택하세요";
        parrotImg.sprite = normalParrot;
    }

    public void OpenSound(SoundSource sound)
    {
        soundController.OpenSound(sound);
    }

    public void ClickNonSound()
    {
        parrotText.text = "거기엔 아무 소리도 들리지 않아요...";
        parrotImg.sprite = sadParrot;
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
}
