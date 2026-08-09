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
    [SerializeField] private GameObject deductionItemCellPrefab;
    [SerializeField] private GameObject deductionPhotoCellPrefab;
    [SerializeField] private GameObject deductionAudioCellPrefab;
    [SerializeField] private Transform deductionItemParent;
    [SerializeField] private Transform deductionPhotoParent;
    [SerializeField] private Transform deductionAudioParent;
    [SerializeField] private Sprite normalBag;
    [SerializeField] private Sprite openedBag;
    [SerializeField] private Image bagImage;
    [SerializeField] private GameObject footButton;
    
    [Header("PhotoUI")]
    [SerializeField] private Transform photoUIParent;
    [SerializeField] private GameObject photoUIPrefab;
    [SerializeField] private GameObject zoomPanel;
    [SerializeField] private Image zoomBackground;
    [SerializeField] private RawImage zoomRawImg;
    [SerializeField] private GameObject[] zoomPanelButtons;
    [SerializeField] private float dissolveDuration = 0.4f;
    [SerializeField] private GameObject imageDescPanel;
    [SerializeField] private GameObject imageDescPrefab;
    [SerializeField] private Transform zoomInRoot;
    
    [Header("ItemUI")]
    [SerializeField] private Transform collectiveUIParent;
    [SerializeField] private GameObject collectiveUIPrefab;
    public TMP_Text nameText;
    public TMP_Text itemDescText;
    
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
    private List<Button> soundObjects = new();
    private Transform[] photoEvidences;
    public Transform[] GetPhotoEvidences() =>  photoEvidences;

    public void SetPhotoEvidences(Transform[] audioEvidences)
    {
        this.photoEvidences = audioEvidences;
    }
    
    void Awake()
    {
        if (instance == null)
            instance = this;

        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;
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

        instance = null;
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

        bagImage.sprite = openedBag;
        bagImage.SetNativeSize();
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
        bagImage.sprite = normalBag;
        bagImage.SetNativeSize();
        bagButton.DOScale(1f, 0.3f);
    }
    
    #region Photo
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
        var o1 = Instantiate(deductionPhotoCellPrefab, deductionPhotoParent);
        o1.GetComponent<SubmitPhotoUICell>().Init(photo);
    }

    public void ZoomInPhoto(int idx)
    {
        currentPhotoIdx = idx;
        zoomRawImg.texture = photos[idx].tex;
        zoomRawImg.SetNativeSize();
        Rect rect = zoomRawImg.rectTransform.rect;

        zoomRawImg.material.SetVector(
            "_RectSize",
            new Vector4(rect.width, rect.height, 0f, 0f)
        );
        foreach (var i in zoomPanelButtons)
            i.SetActive(true);
        for (int i = 0; i != imageDescPanel.transform.childCount; ++i)
        {
            Destroy(imageDescPanel.transform.GetChild(i).gameObject);
        }
        if (photos[idx].descs.Count > 0)
        {
            imageDescPanel.SetActive(true);
            foreach (var i in photos[idx].descs)
            {
                zoomInRoot.DOLocalMoveX(-437, 0f);
                var o = Instantiate(imageDescPrefab, imageDescPanel.transform);
                var rt = o.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                // 상하좌우 모두 Stretch
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;

                // 좌우 여백 20, 상하 여백 0
                rt.offsetMin = new Vector2(20f, 0f);   // Left, Bottom
                rt.offsetMax = new Vector2(-20f, 0f); // -Right, -Top
                o.GetComponent<TMP_Text>().text = i;
            }
        }
        else
        {
            zoomInRoot.DOLocalMoveX(-117, 0f);
            imageDescPanel.SetActive(false);
        }
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
        AudioManager.Instance.PlaySFX(SFXType.ChamielDelete);
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
        zoomPanel.SetActive(false);
        int idx = currentPhotoIdx;
        if (photos[idx].tex != null)
        {
            Destroy(photos[idx].tex);
            photos[idx].tex = null;
        }
        photos.RemoveAt(idx);
        Destroy(photoUIParent.transform.GetChild(idx).gameObject);
        Destroy(deductionPhotoParent.transform.GetChild(idx).gameObject);
        
        await zoomBackground.DOFade(0f, 0.3f).ToUniTask();
        dissolveMaterial.SetFloat(DissolveAmountId, 0f);
    }
    #endregion

    public void AddCollectible(CollectiveEvidence collective)
    {
        collectives.Add(collective);
        // UI 프리펩 생성
        var o = Instantiate(collectiveUIPrefab, collectiveUIParent);
        o.GetComponent<CollectiveUICell>().Init(collective);
        var o1 = Instantiate(deductionItemCellPrefab, deductionItemParent);
        o1.GetComponent<SubmitItemUICell>().Init(collective);
    }
    
    #region Audio
    public void AddSoundButton(Button b)
    {
        soundObjects.Add(b);
    }

    public void DeleteSoundButton(Button b)
    {
        soundObjects.Remove(b);
    }

    public void EnterSoundMode()
    {
        if (!TutorialManager.instance.isParrotTutorialCleared)
        {
            TutorialManager.instance.ParrotTutorial();
            return;
        }

        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        footButton.SetActive(false);
        parrotText.text = "기억할 소리를 선택하세요";
        parrotImg.sprite = normalParrot;
        parrotImg.SetNativeSize();
        soundNoneArea.SetActive(true);
        exitSoundButton.SetActive(true);
        foreach(var o in hideObjects)
            o.SetActive(false);
        foreach (var b in soundObjects)
            b.enabled = true;
    }

    public void ExitSoundMode()
    {
        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;
        soundNoneArea.SetActive(false);
        exitSoundButton.SetActive(false);
        footButton.SetActive(true);
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
        var o1 = Instantiate(deductionAudioCellPrefab, deductionAudioParent);
        o1.GetComponent<SubmitAudioUICell>().Init(sound);
        parrotText.text = "기억할 소리를 선택하세요";
        parrotImg.sprite = normalParrot;
        parrotImg.SetNativeSize();
    }

    public void OpenSound(SoundSource sound)
    {
        soundController.OpenSound(sound);
    }

    public void ClickNonSound()
    {
        AudioManager.Instance.PlaySFX(SFXType.AngeleNotFound);
        parrotText.text = "거기엔 아무 소리도 들리지 않아요...";
        parrotImg.sprite = sadParrot;
        parrotImg.SetNativeSize();
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && GameManager.Instance.CanUseInventory)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isInventoryOpening) return;
        isInventoryOpening = true;
        
        if (isInventoryOpened)
        {
            AudioManager.Instance.PlaySFX(SFXType.BagClose);
            footButton.SetActive(true);
            bagImage.sprite = normalBag;
            bagImage.SetNativeSize();
            GameManager.Instance.CanUseOption = true;
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
            AudioManager.Instance.PlaySFX(SFXType.BagOpen);
            footButton.SetActive(false);
            bagImage.sprite = openedBag;
            bagImage.SetNativeSize();
            GameManager.Instance.CanUseOption = false;
            isInventoryOpened = true;
            background.gameObject.SetActive(true);
            inventoryParent.SetActive(true);
            background.DOFade(250.0f / 255f, 0.3f).OnComplete(() => isInventoryOpening = false);
        }
    }

    private GameObject moveButtons;
    private bool isMove;

    public void SetMoveButtons(GameObject o)
    {
        moveButtons = o;
    }

    public void SetSoundNonArea(GameObject o, Image parrotImage, TMP_Text parrotText)
    {
        soundNoneArea = o;
        parrotImg = parrotImage;
        this.parrotText = parrotText;
    }

    public void ToggleMove()
    {
        if (isMove)
        {
            ExitMoveMode();
        }
        else
        {
            EnterMoveMode();
        }
    }

    public void EnterMoveMode()
    {
        isMove = true;
        moveButtons.SetActive(true);
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        foreach(var o in hideObjects)
            o.SetActive(false);
    }

    public void ExitMoveMode()
    {
        isMove = false;
        moveButtons.SetActive(false);
        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;
        foreach(var o in hideObjects)
            o.SetActive(true);
    }
}
