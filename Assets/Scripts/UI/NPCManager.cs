using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private string clientName = "의뢰인";
    [SerializeField] private Sprite clientSprite;
    [SerializeField] private Sprite monkeySprite;
    [SerializeField] private CaseSolution solution;
    [SerializeField] private float[] evaluationWeights;
    private FinalDeductionResult deductionOutput;
    
    [Header("Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private DialogueTextAnimator textAnim;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portrait;

    [Header("Deduction")] 
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Image confirmBackground;
    
    [SerializeField] private GameObject deductionResultPanel;
    [SerializeField] private Image deductionResultBackground;
    [SerializeField] private TMP_Text deductionResultText;
    
    [SerializeField] private GameObject submitCellPrefab;
    [SerializeField] private Transform submitParent;
    
    [SerializeField] private GameObject[] step1Objects;
    [SerializeField] private GameObject[] step2Objects;

    [SerializeField] private GameObject[] step2Buttons;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject step2DialoguePanel;
    [SerializeField] private GameObject inputDialogue;
    [SerializeField] private DialogueTextAnimator inputDialogueText;
    [SerializeField] private GameObject loadingDialogue;
    [SerializeField] private TextWaveAnimator loadingTextAnimator;

    [SerializeField] private Image finalLoadingPanel;
    [SerializeField] private BackgroundFacts backgroundFacts;

    [SerializeField] private GameObject resultCanvas;
    [SerializeField] private Image resultBackground;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image resultSticker;
    [SerializeField] private ResultData resultData;
    
    public static NPCManager instance;
    private OriginalEvidenceRecord originalEvidenceRecord = new OriginalEvidenceRecord();
    private Tween backgroundTween;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else return;

        originalEvidenceRecord.photos = new();
        originalEvidenceRecord.audios = new();
        originalEvidenceRecord.collectiveEvidences = new();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public UniTask PlayDialogueDetective(List<string> dialogue)
    {
        return PlayDialogue(dialogue, monkeySprite, "미스터 A");
    }

    public async UniTask PlayDialogue(List<string> dialogue, Sprite sprite, String name, bool isNpcDialogue = false)
    {
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        
        backgroundTween?.Kill();
        background.gameObject.SetActive(true);
        background.DOFade(250.0f / 255f, 0.3f);
        
        nameText.text = name;
        portrait.sprite = sprite;
        portrait.SetNativeSize();
        dialoguePanel.SetActive(true);
        foreach (var s in dialogue)
        {
            await textAnim.PlayDialogueAsync(s);
            await UniTask.Yield();
        }
        dialoguePanel.SetActive(false);
        backgroundTween = background.DOFade(0f, 0.3f).OnComplete(()=>background.gameObject.SetActive(false));

        if (isNpcDialogue)
        {
            GameManager.Instance.CanUseInventory = true;
            GameManager.Instance.CanUseOption = true;
        }
    }

    public void AskDeduction()
    {
        GameManager.Instance.CanUseInventory = false;
        GameManager.Instance.CanUseOption = false;
        
        EventSystem.current?.SetSelectedGameObject(null);
        AskDeductionAsync().Forget();
    }
    
    private async UniTaskVoid AskDeductionAsync()
    {
        List<string> dialogue = new()
        {
            "레시피 노트를 도둑맞았다고 생각했는데... 다시 보니 책장에 꽂혀는 있었어요.",
            "하지만 제가 늘 꽂아두는 곳이 아닌 완전히 다른 곳에 있는 게 좀 이상하긴 했죠.",
            "그나저나...",
            "설마 벌써 범인을 알아내신 건가요?!"
        };

        await PlayDialogue(dialogue, clientSprite, clientName);
        ShowConfirmPanel();
    }

    public void CloseConfirmPanel()
    {
        confirmPanel.SetActive(false);
        confirmBackground.gameObject.SetActive(false);
        
        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;
    }

    private void ShowConfirmPanel()
    {
        confirmPanel.SetActive(true);
        confirmBackground.gameObject.SetActive(true);
        confirmBackground.DOFade(0f, 0f);
        confirmBackground.DOFade(250.0f / 255f, 0.3f);
    }

    public void EnterStep1()
    {
        confirmPanel.SetActive(false);
        AudioManager.Instance.PlayBGM(BGMType.Deduction);
        background.gameObject.SetActive(true);
        background.DOFade(250.0f / 255f, 0.3f);
        
        foreach(var o in step2Objects)
            o.SetActive(false);
        foreach(var o in step1Objects)
            o.SetActive(true);

        var bs = submitParent.GetComponentsInChildren<Button>();
        foreach (var b in bs)
        {
            b.interactable = true;
            if (b.transform.childCount > 0)
            {
                Transform lastChild = b.transform.GetChild(b.transform.childCount - 1);
                var component = lastChild.GetComponent<Image>();
                component.gameObject.SetActive(true);
            }
        }
    }

    public void ExitStep1()
    {
        foreach(var o in step1Objects)
            o.SetActive(false);
        
        GameManager.Instance.CanUseInventory = true;
        GameManager.Instance.CanUseOption = true;
        AudioManager.Instance.PlayBGM(BGMType.Ppuang);
        background.DOFade(0f, 0.3f).OnComplete(()=>background.gameObject.SetActive(false));
    }

    public void AddEvidence(CollectiveEvidence evidence, Button button, Vector3 from)
    {
        originalEvidenceRecord.collectiveEvidences.Add(evidence);
        var o = Instantiate(submitCellPrefab, submitParent);
        o.GetComponent<SubmitUICell>().Init(evidence, button);
        AddEvidenceAnim(from, o);
    }

    public void RemoveEvidence(CollectiveEvidence evidence)
    {
        originalEvidenceRecord.collectiveEvidences.Remove(evidence);
    }
    
    public void AddEvidence(SoundSource evidence, Button button, Vector3 from)
    {
        originalEvidenceRecord.audios.Add(evidence);
        var o = Instantiate(submitCellPrefab, submitParent);
        o.GetComponent<SubmitUICell>().Init(evidence, button);
        AddEvidenceAnim(from, o);
    }

    public void RemoveEvidence(SoundSource evidence)
    {
        originalEvidenceRecord.audios.Remove(evidence);
    }
    
    public void AddEvidence(PhotoData evidence, Button button, Vector3 from)
    {
        originalEvidenceRecord.photos.Add(evidence);
        var o = Instantiate(submitCellPrefab, submitParent);
        o.GetComponent<SubmitUICell>().Init(evidence, button);
        AddEvidenceAnim(from, o);
    }

    public void RemoveEvidence(PhotoData evidence)
    {
        originalEvidenceRecord.photos.Remove(evidence);
    }

    private void AddEvidenceAnim(Vector3 from, GameObject o)
    {
        /*
        var to = o.transform.position;
        o.transform.position = from;
        o.SetActive(true);
        o.transform.DOMove(to, 0.7f).SetEase(Ease.InOutSine);
        */
        o.SetActive(true);
    }

    public void EnterStep2()
    {
        foreach(var o in step1Objects)
            o.SetActive(false);
        foreach(var o in step2Objects)
            o.SetActive(true);
        inputField.gameObject.SetActive(true);
        var bs = submitParent.GetComponentsInChildren<Button>();
        foreach (var b in bs)
        {
            b.interactable = false;
            if (b.transform.childCount > 0)
            {
                Transform lastChild = b.transform.GetChild(b.transform.childCount - 1);
                var component = lastChild.GetComponent<Image>();
                component.gameObject.SetActive(false);
            }
        }
    }

    public void SubmitRecord()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        if (originalEvidenceRecord.collectiveEvidences.Count == 0 && originalEvidenceRecord.photos.Count == 0 && originalEvidenceRecord.audios.Count == 0) return;
        EventSystem.current?.SetSelectedGameObject(null);
        SubmitRecordAsync().Forget();
    }

    private async UniTaskVoid SubmitRecordAsync()
    {
        foreach(var o in step2Buttons)
            o.SetActive(false);
        inputField.gameObject.SetActive(false);
        originalEvidenceRecord.playerDescription = inputField.text;
        step2DialoguePanel.SetActive(true);
        loadingDialogue.SetActive(true);
        loadingTextAnimator.Play("음....");
        var input = new EvidenceValidationInput();
        input.evidenceDatas = new();
        foreach(var i in originalEvidenceRecord.collectiveEvidences)
            input.evidenceDatas.Add(i.data);
        foreach(var i in originalEvidenceRecord.photos)
            input.evidenceDatas.AddRange(i.datas);
        foreach(var i in originalEvidenceRecord.audios)
            input.evidenceDatas.Add(i.data);
        input.playerDescription = inputField.text;
        
        var output = await AISystemManager.Instance.AI.InputValidator.ValidateAsync(input);
        loadingTextAnimator.Stop();
        
        loadingDialogue.SetActive(false);
        inputDialogue.SetActive(true);
        await inputDialogueText.PlayDialogueAsync(output.response);

        if (output.GetStatus() == InputStatus.Accept)
        {
            DoDeduction().Forget();
        }
        else
        {
            inputDialogue.SetActive(false);
            step2DialoguePanel.SetActive(false);
            inputField.gameObject.SetActive(true);
            foreach(var o in step2Objects)
                o.SetActive(true);
        }
    }

    private int noneCount = 0;

    private async UniTaskVoid DoDeduction()
    {
        inputDialogue.SetActive(false);
        step2DialoguePanel.SetActive(false);
        foreach(var o in step2Objects)
            o.SetActive(false);
        background.DOFade(0f, 0f);
        background.gameObject.SetActive(false);
        finalLoadingPanel.gameObject.SetActive(true);
        finalLoadingPanel.DOFade(250.0f / 255f, 0.3f);

        var input = new FinalDeductionInput();
        input.evidenceRecords = new();
        var record = new EvidenceRecord();
        record.evidences = new();
        foreach(var i in originalEvidenceRecord.collectiveEvidences)
            record.evidences.Add(i.data);
        foreach(var i in originalEvidenceRecord.photos)
            record.evidences.AddRange(i.datas);
        foreach(var i in originalEvidenceRecord.audios)
            record.evidences.Add(i.data);
        record.playerDescription = inputField.text;
        input.evidenceRecords.Add(record);
        input.backgroundFacts = backgroundFacts.Facts;
        deductionOutput = await AISystemManager.Instance.AI.Detective.DeduceAsync(input);
        
        finalLoadingPanel.gameObject.SetActive(false);

        var s = new List<string>()
        {
            "조수들은 이 진술에 대해 어떻게 생각하지?"
        };
        await PlayDialogue(s, monkeySprite, "미스터 A");
        noneCount = 0;
        deductionResultText.text = "범인 : " + deductionOutput.culprit + "\n" + "\n" +
                                   "동기 : " + deductionOutput.motive + "\n" + "\n";
        if (deductionOutput.culprit == "불명확" || deductionOutput.culprit == "해당 없음")
            ++noneCount;
        if (deductionOutput.motive == "불명확" || deductionOutput.motive == "해당 없음")
            ++noneCount;
        string method = "불명확";
        string methodString = "";
        if (deductionOutput.time != "불명확" && deductionOutput.time != "해당 없음")
            methodString += deductionOutput.scene + "\n";
        else
            ++noneCount;
        if (deductionOutput.time != "불명확" && deductionOutput.time != "해당 없음")
            methodString += deductionOutput.time + "\n";
        else
            ++noneCount;
        if (deductionOutput.accessMethod != "불명확" && deductionOutput.accessMethod != "해당 없음")
            methodString += deductionOutput.accessMethod + "\n";
        else
            ++noneCount;
        if (deductionOutput.coreAction != "불명확" && deductionOutput.coreAction != "해당 없음")
            methodString += deductionOutput.coreAction + "\n";
        else
            ++noneCount;
        if (deductionOutput.originalStatus != "불명확" && deductionOutput.originalStatus != "해당 없음")
            methodString += deductionOutput.originalStatus + "\n";
        else
            ++noneCount;
        if (deductionOutput.copyDestination != "불명확" && deductionOutput.copyDestination != "해당 없음")
            methodString += deductionOutput.copyDestination + "\n";
        else
            ++noneCount;
        if (deductionOutput.tasteGapReason != "불명확" && deductionOutput.tasteGapReason != "해당 없음")
            methodString += deductionOutput.tasteGapReason;
        else
            ++noneCount;
        if (!string.IsNullOrEmpty(methodString))
            method = methodString;
        deductionResultText.text += "수법 : " + method;
        
        deductionResultBackground.gameObject.SetActive(true);
        deductionResultBackground.DOFade(0f, 0f);
        deductionResultBackground.DOFade(250.0f / 255f, 0.3f);
        deductionResultPanel.SetActive(true);
    }

    public void AcceptDeduction()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        AcceptDeductionAsync().Forget();
    }
    
    private async UniTaskVoid AcceptDeductionAsync()
    {
        deductionResultPanel.SetActive(false);
        
        var s = new List<string>()
        {
            "좋아요. 모든 퍼즐을 맞췄습니다."
        };
        s.AddRange(SplitSentences(deductionOutput.narrative));

        var input = new DeductionEvaluationInput();
        input.deduction =  deductionOutput;
        input.solution = solution;
        var t = AISystemManager.Instance.AI.Evaluator.EvaluateAsync(input);
        
        await PlayDialogue(s, monkeySprite, "미스터 A");
        var result = await t;

        float score = CalculateScore(result);
        List<string> response;
        Sprite sticker;
        SFXType type = SFXType.BadEnding;
        if (score < resultData.NormalScore)
        {
            response = resultData.BadResponse;
            sticker = resultData.BadSticker;
            type = SFXType.BadEnding;
        }
        else if (score < resultData.GoodScore)
        {
            response = resultData.NormalResponse;
            sticker = resultData.NormalSticker;
            type =  SFXType.NormalEnding;
        }
        else
        {
            response = resultData.GoodResponse;
            sticker = resultData.GoodSticker;
            type = SFXType.GoodEnding;
        }
        AudioManager.Instance.StopBGM();

        await UniTask.NextFrame();
        await PlayDialogue(response, clientSprite, clientName);
        
        resultCanvas.gameObject.SetActive(true);
        resultBackground.DOFade(0f, 0f);
        resultBackground.DOFade(250.0f / 255f, 0.3f);
        float sum = 0;
        foreach (var i in evaluationWeights)
            sum += i;
        resultText.text = (score*100f).ToString() + " 점 / " + (sum*100f).ToString() + " 점";
        AudioManager.Instance.PlaySFX(SFXType.EndingStamp);
        AudioManager.Instance.PlaySFX(type);
        resultSticker.DOFade(0f, 0f);
        resultSticker.DOFade(1f, 0.3f);
        resultSticker.sprite = sticker;
        resultSticker.SetNativeSize();
    }

    private float CalculateScore(DeductionEvaluationResult result)
    {
        float sum = 0;
        sum += result.culpritScore * evaluationWeights[0];
        sum += result.motiveScore * evaluationWeights[1];
        sum += result.sceneScore * evaluationWeights[2];
        sum += result.timeScore * evaluationWeights[3];
        sum += result.accessMethodScore * evaluationWeights[4];
        sum += result.coreActionScore * evaluationWeights[5];
        sum += result.originalStatusScore * evaluationWeights[6];
        sum += result.copyDestinationScore * evaluationWeights[7];
        sum += result.tasteGapReasonScore * evaluationWeights[8];

        float misleadingPenalty = solution.misleadingClaims
            .Where(claim =>
                result.detectedMisleadingClaims.Contains(claim.claimId))
            .Sum(claim => claim.penalty);

        float finalScore = Mathf.Clamp01(sum - misleadingPenalty);
        if (noneCount >= 3)
            finalScore = Mathf.Clamp01(finalScore - 0.3f);
        return finalScore;
    }
    
    private static List<string> SplitSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        const string pattern = @"[^.!?。！？…\r\n]+[.!?。！？…]*";

        return Regex.Matches(text, pattern)
            .Select(match => match.Value.Trim())
            .Where(sentence => !string.IsNullOrEmpty(sentence))
            .ToList();
    }
}
