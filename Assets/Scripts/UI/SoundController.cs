using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
     [SerializeField] private Image background;
     [SerializeField] private GameObject rootObject;
     [SerializeField] private GameObject textGroup;
     [SerializeField] private Image soundImage;
     [SerializeField] private TMP_Text soundText;
     [SerializeField] private Image parrotImage;
     [SerializeField] private Sprite hearingParrot;
     [SerializeField] private Sprite normalParrot;
     [SerializeField] private float waitTime = 1.5f;
     [SerializeField] private float moveDis = 25f;
     [SerializeField] private float moveTime = 0.7f;

     public void CollectSound(SoundSource sound)
     {
          textGroup.SetActive(true);
          parrotImage.sprite = hearingParrot;
          OpenPanel(sound).Forget();
     }

     public void OpenSound(SoundSource sound)
     {
          textGroup.SetActive(false);
          parrotImage.sprite = normalParrot;
          OpenPanel(sound).Forget();
     }

     private async UniTaskVoid OpenPanel(SoundSource sound)
     {
          background.gameObject.SetActive(true);
          background.DOFade(250.0f / 255f, 0.3f);
          rootObject.SetActive(true);
          soundText.text = sound.data.fact;
          soundImage.sprite = sound.sprite;
          soundImage.SetNativeSize();
          var originalPos = soundImage.transform.position;
          var tween = soundImage.transform.DOMoveY(soundImage.transform.position.y + moveDis, moveTime).SetLoops(-1, LoopType.Yoyo);
          // TODO : 오디오 클립 재생 (살짝 높은 피치로)
          await UniTask.WaitForSeconds(waitTime);
          background.DOFade(0f, 0.3f).OnComplete(()=>background.gameObject.SetActive(false));
          rootObject.SetActive(false);
          tween.Kill();
          soundImage.transform.position = originalPos;
          soundText.text = "";
     }
}
