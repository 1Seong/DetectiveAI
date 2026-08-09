using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset dialogueCsv;

    [SerializeField] private DialogueTextAnimator textAnimator;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text speaker;
    [SerializeField] private GameObject tag;
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image rightCharacter;
    [SerializeField] private float shrinkValue = 0.85f;
    [SerializeField] private Color fadeColor;
    [SerializeField] private float initialWaitingTime = 2f;
    [SerializeField] private Image tutorialBackground;

    private List<DialogueData> dialogueDatas;
    private Dictionary<string, Sprite> sprites = new();

    private void Awake()
    {
        dialogueDatas = DialogueCsvParser.Parse(dialogueCsv);

        foreach (DialogueData data in dialogueDatas)
        {
            Debug.Log(
                $"ID: {data.ID}\n" +
                $"Speaker: {data.Speaker}\n" +
                $"Dialogue: {data.Dialogue}\n" +
                $"NextID: {data.NextID}"
            );
        }
    }

    private int idx;

    private async void Start()
    {
        await UniTask.WaitForSeconds(initialWaitingTime);
        
        for(int i = 0; i < dialogueDatas.Count; i++)
        {
            await PlayDialogue(dialogueDatas[i]);
            if (dialogueDatas[i].ID[^1] == 'T')
            {
                idx = i+1;
                break;
            }
        }

        tutorialBackground.DOFade(250.0f / 255f, 0.3f);
        tutorialBackground.gameObject.SetActive(true);
    }

    public void ContinueDialogue()
    {
        ContinueDialogueAsync().Forget();
    }

    private async UniTask ContinueDialogueAsync()
    {
        for(int i = idx; i < dialogueDatas.Count; i++)
        {
            await PlayDialogue(dialogueDatas[i]);
        }
        
        EnterGame();
    }

    public void EnterGame()
    {
        SceneTransitionManager.Instance.ChangeSceneAsync("SampleScene").Forget();
    }

    private UniTask PlayDialogue(DialogueData data)
    {
        if (!string.IsNullOrEmpty(data.Background))
        {
            background.sprite = Resources.Load<Sprite>(data.Background);
        }
        
        if (!string.IsNullOrEmpty(data.LeftCharacter))
        {
            leftCharacter.gameObject.SetActive(true);
            var s = sprites.ContainsKey(data.LeftCharacter) ? sprites[data.LeftCharacter] : Resources.Load<Sprite>(data.LeftCharacter);
            if (s == null)
            {
                if(data.LeftCharacter.Contains("mrA"))
                    s = Resources.Load<Sprite>("mrA");
                else if(data.LeftCharacter.Contains("angele"))
                    s = Resources.Load<Sprite>("angele");
                else if(data.LeftCharacter.Contains("chamiel"))
                    s = Resources.Load<Sprite>("chamiel");
                else if(data.LeftCharacter.Contains("ppuang"))
                    s = Resources.Load<Sprite>("ppuang");
            }

            sprites[data.LeftCharacter] = s;

            leftCharacter.sprite = s;
            leftCharacter.SetNativeSize();
        }
        else
        {
            leftCharacter.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(data.RightCharacter))
        {
            rightCharacter.gameObject.SetActive(true);
            var s = sprites.ContainsKey(data.RightCharacter) ? sprites[data.RightCharacter] : Resources.Load<Sprite>(data.RightCharacter);
            if (s == null)
            {
                if(data.RightCharacter.Contains("mrA"))
                    s = Resources.Load<Sprite>("mrA");
                else if(data.RightCharacter.Contains("angele"))
                    s = Resources.Load<Sprite>("angele");
                else if(data.RightCharacter.Contains("chamiel"))
                    s = Resources.Load<Sprite>("chamiel");
                else if(data.RightCharacter.Contains("ppuang"))
                    s = Resources.Load<Sprite>("ppuang");
            }
            
            sprites[data.RightCharacter] = s;

            rightCharacter.sprite = s;
            rightCharacter.SetNativeSize();
        }
        else
        {
            rightCharacter.gameObject.SetActive(false);
        }
        
        if (string.IsNullOrEmpty(data.Speaker))
        {
            tag.SetActive(false);
            rightCharacter.DOColor(fadeColor, 0.3f);
            rightCharacter.transform.DOScale(shrinkValue, 0.3f);
            leftCharacter.DOColor(fadeColor, 0.3f);
            leftCharacter.transform.DOScale(new Vector3(-shrinkValue, shrinkValue, shrinkValue), 0.3f);
        }
        else if (data.Speaker == "Left")
        {
            tag.SetActive(true);
            SetName(data.LeftCharacter);
            rightCharacter.DOColor(fadeColor, 0.3f);
            rightCharacter.transform.DOScale(shrinkValue, 0.3f);
            leftCharacter.DOColor(Color.white, 0.3f);
            leftCharacter.transform.DOScale(new Vector3(-1, 1, 1), 0.3f);
        }
        else if (data.Speaker == "Right")
        {
            tag.SetActive(true);
            SetName(data.RightCharacter);
            rightCharacter.DOColor(Color.white, 0.3f);
            rightCharacter.transform.DOScale(1f, 0.3f);
            leftCharacter.DOColor(fadeColor, 0.3f);
            leftCharacter.transform.DOScale(new Vector3(-shrinkValue, shrinkValue, shrinkValue), 0.3f);
        }

        return textAnimator.PlayDialogueAsync(data.Dialogue);
    }

    private void SetName(string spriteName)
    {
        if (spriteName.Contains("mrA"))
        {
            speaker.text = "미스터 A";
        }
        else if (spriteName.Contains("angele"))
        {
            speaker.text = "앵젤";
        }
        else if (spriteName.Contains("ppuang"))
        {
            speaker.text = "쁘앙";
        }
        else if (spriteName.Contains("chamiel"))
        {
            speaker.text = "카미엘";
        }
    }
}
