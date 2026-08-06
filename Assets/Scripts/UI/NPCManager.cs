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

    public async UniTask PlayDialogue(List<string> dialogue, Sprite sprite, String name)
    {
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
            
            // 현재 문장을 넘긴 스페이스 입력이
            // 다음 문장까지 전달되지 않도록 기다린다.
            await UniTask.WaitUntil(
                () => !Input.GetKey(KeyCode.Space)
            );

            await UniTask.Yield();
        }
        dialoguePanel.SetActive(false);
        backgroundTween = background.DOFade(0f, 0.3f).OnComplete(()=>background.gameObject.SetActive(false));
    }

    public void AskDeduction()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        AskDeductionAsync().Forget();
    }
    
    private async UniTaskVoid AskDeductionAsync()
    {
        List<string> dialogue = new()
        {
            "도대체 누가 이런 짓을...",
            "혹시 범인을 알아내신 건가요?!"
        };

        await PlayDialogue(dialogue, clientSprite, clientName);
        ShowConfirmPanel();
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
        
        background.gameObject.SetActive(true);
        background.DOFade(250.0f / 255f, 0.3f);
        
        foreach(var o in step2Objects)
            o.SetActive(false);
        foreach(var o in step1Objects)
            o.SetActive(true);

        var bs = submitParent.GetComponentsInChildren<Button>();
        foreach (var b in bs)
            b.interactable = true;
    }

    public void ExitStep1()
    {
        foreach(var o in step1Objects)
            o.SetActive(false);
        
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
            b.interactable = false;
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
        
        finalLoadingPanel.DOFade(0f, 0.3f).OnComplete(()=>finalLoadingPanel.gameObject.SetActive(false));

        var s = new List<string>()
        {
            "조수들은 이 진술에 대해 어떻게 생각하지?"
        };
        await PlayDialogue(s, monkeySprite, "탐정");

        deductionResultText.text = "범인 : " + deductionOutput.culprit + "\n" +
                                   "동기 : " + deductionOutput.motive + "\n" +
                                   "수법 : " + deductionOutput.method;
        // TODO : 출력 형식 수정
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
        
        await PlayDialogue(s, monkeySprite, "탐정");
        var result = await t;

        float score = CalculateScore(result);
        List<string> response;
        Sprite sticker;
        if (score < resultData.NormalScore)
        {
            response = resultData.BadResponse;
            sticker = resultData.BadSticker;
        }
        else if (score < resultData.GoodScore)
        {
            response = resultData.NormalResponse;
            sticker = resultData.NormalSticker;
        }
        else
        {
            response = resultData.GoodResponse;
            sticker = resultData.GoodSticker;
        }

        await UniTask.NextFrame();
        await PlayDialogue(response, clientSprite, clientName);
        
        resultCanvas.gameObject.SetActive(true);
        resultBackground.DOFade(0f, 0f);
        resultBackground.DOFade(250.0f / 255f, 0.3f);
        resultText.text = score.ToString() + " 점 / 3 점"; // TODO : 총점 수정
        resultSticker.sprite = sticker;
        resultSticker.SetNativeSize();
    }

    private float CalculateScore(DeductionEvaluationResult result)
    {
        float sum = 0;
        sum += result.culpritScore * evaluationWeights[0];
        sum += result.motiveScore * evaluationWeights[1];
        sum += result.methodScore * evaluationWeights[2];
        // TODO : 계산식 수정
        return sum;
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
