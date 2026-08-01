using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CollectiveEvidence : MonoBehaviour
{
    public Sprite sprite;
    public EvidenceData data;

    [SerializeField] private float moveUpDis = 1f;
    [SerializeField] private float moveUpDur = 0.3f;
    [SerializeField] private float moveToBagDur = 0.7f;

    public void OnClick()
    {
        InventoryManager.instance.AddCollectible(this);
        
        GetComponent<Button>().enabled = false;
        var seq =  DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + moveUpDis, moveUpDur).SetEase(Ease.OutBack));
        seq.AppendCallback(InventoryManager.instance.ScaleUpBagButton);
        seq.Append(transform.DOMove(InventoryManager.instance.GetBagButtonPos(), moveToBagDur).SetEase(Ease.InCubic));
        seq.Join(transform.DOScale(transform.localScale.x * 0.2f, moveToBagDur).SetEase(Ease.InCubic).OnComplete(()=>gameObject.SetActive(false)));
        seq.AppendCallback(InventoryManager.instance.ScaleDownBagButton);
    }
}
