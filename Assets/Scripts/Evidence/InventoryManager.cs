using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [SerializeField] private Image blackBackground;
    [SerializeField] private EvidenceRecorder evidenceRecorder;
    
    [Header("PhotoUI")]
    [SerializeField] private Transform photoUIParent;
    [SerializeField] private GameObject photoUIPrefab;
    
    [Header("ItemUI")]
    [SerializeField] private Transform collectiveUIParent;
    [SerializeField] private GameObject collectiveUIPrefab;
    
    [Header("SubmitUI")]
    [SerializeField] private Transform collectiveSubmitUIParent;
    [SerializeField] private GameObject collectiveSubmitUIPrefab;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private List<PhotoData> photos;
    private List<CollectiveEvidence> collectives;

    private int currentPhotoIndex;

    public void AddPhoto(PhotoData photo)
    {
        photos.Add(photo);
        // UI 프리펩 생성
        // ui 이미지에 이미지 넣기
        // 버튼에 zoomphoto 래핑 람다 콜백 넣기
    }

    public void AddCollectible(CollectiveEvidence collective)
    {
        collectives.Add(collective);
        // UI 프리펩 생성
        // ui 이미지에 이미지 넣기
    }
    
    #region PhotoUI
    public void ShowPhotos()
    {
        // ui parent에 있는 사진들을 펼쳐서 보여주기
        // scale을 1로, 위치를 격자로
    }

    public void ZoomPhoto(int i)
    {
        // 사진들을 가로로 확대해서 나열하고
        // 좌우 버튼 활성화
        // 삭제 버튼 활성화
    }

    public void ShowLeft()
    {
        
    }

    public void ShowRight()
    {
        
    }

    public void DeletePhoto()
    {
        photos.RemoveAt(currentPhotoIndex);
        Destroy(photoUIParent.GetChild(currentPhotoIndex).gameObject);
    }
    #endregion
    
    #region CollectiveUI
    
    #endregion
    
    #region SubmitUI

    public void ShowSubmitUI()
    {
        
    }

    public void SelectPhoto()
    {
        
    }

    public void SelectCollectives()
    {
        
    }

    public void Submit()
    {
        
    }
    
    #endregion
}
