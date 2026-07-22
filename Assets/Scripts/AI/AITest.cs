using System.Diagnostics;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AITest : MonoBehaviour
{
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private EvidenceValidationInput[] inputs1;
    [SerializeField] private FinalDeductionInput[] inputs2;
    [SerializeField] private DeductionEvaluationInput[] inputs3;
    
    public async void InputValidationAITest()
    {
        foreach (var i in inputs1)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var r = await AISystemManager.Instance.AI.InputValidator.ValidateAsync(i);
            
            stopwatch.Stop();

            Debug.Log(
                $"검증 완료: {stopwatch.ElapsedMilliseconds}ms\n{r}");
            Debug.Log(r.ToString());
        }
    }

    public async void DetectiveAITest()
    {
        foreach (var i in inputs2)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var r = await AISystemManager.Instance.AI.Detective.DeduceAsync(i);
            
            stopwatch.Stop();
            Debug.Log(
                $"검증 완료: {stopwatch.ElapsedMilliseconds}ms\n{r}");
            Debug.Log(r.ToString());
        }
    }

    public async void EvaluationAITest()
    {
        foreach (var i in inputs3)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var r = await AISystemManager.Instance.AI.Evaluator.EvaluateAsync(i);
            stopwatch.Stop();
            Debug.Log(
                $"검증 완료: {stopwatch.ElapsedMilliseconds}ms\n{r}");
            Debug.Log(r.ToString());
        }
    }
}
