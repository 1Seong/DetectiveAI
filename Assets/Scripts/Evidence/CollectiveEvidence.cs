using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CollectiveEvidence : MonoBehaviour
{
    public Sprite sprite;
    [TextArea]
    public string desc;
    public EvidenceData data;

    [SerializeField] private float moveUpDis = 1f;
    [SerializeField] private float moveUpDur = 0.3f;
    [SerializeField] private float moveToBagDur = 0.7f;

    public void OnClick()
    {
        InventoryManager.instance.AddCollectible(this);
        
        GetComponent<Button>().enabled = false;
        GetComponent<ButtonHoverScale>().enabled = false;
        var seq =  DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + moveUpDis, moveUpDur).SetEase(Ease.OutBack));
        seq.AppendCallback(InventoryManager.instance.ScaleUpBagButton);
        seq.Append(transform.DOMove(InventoryManager.instance.GetBagButtonPos(), moveToBagDur).SetEase(Ease.InCubic));
        seq.Join(transform.DOScale(0f, moveToBagDur).SetEase(Ease.InCubic).OnComplete(()=>gameObject.SetActive(false)));
        seq.Join(GetComponent<Image>().DOFade(0f, moveToBagDur).SetEase(Ease.InCubic));
        seq.AppendCallback(InventoryManager.instance.ScaleDownBagButton);
    }
}
