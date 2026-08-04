using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

[Serializable]
public class DeductionEvaluationInput
{
    public FinalDeductionResult deduction;
    public CaseSolution solution;
}

[Serializable]
public class DeductionEvaluationResult
{
    public float culpritScore;
    public float methodScore;
    public float motiveScore;
    public float keyPointScore;

    public List<string> matchedPoints;
    public List<string> detectedMisleadingClaims;
    
    public override string ToString()
    {
        string matchedPointsText =
            matchedPoints == null || matchedPoints.Count == 0
                ? "없음"
                : "- " + string.Join("\n- ", matchedPoints);

        string misleadingClaimsText =
            detectedMisleadingClaims == null ||
            detectedMisleadingClaims.Count == 0
                ? "없음"
                : "- " + string.Join("\n- ", detectedMisleadingClaims);

        return
            $"Culprit Score: {culpritScore:F2}\n" +
            $"Method Score: {methodScore:F2}\n" +
            $"Motive Score: {motiveScore:F2}\n" +
            $"Key Point Score: {keyPointScore:F2}\n" +
            $"Matched Points:\n{matchedPointsText}\n" +
            $"Detected Misleading Claims:\n{misleadingClaimsText}";
    }
}
 
public class DeductionEvaluationAI
{
    private readonly AIEdgeFunctionClient client;

    public DeductionEvaluationAI(AIEdgeFunctionClient client)
    {
        this.client = client;
    }

    public UniTask<DeductionEvaluationResult> EvaluateAsync(
        DeductionEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        return client.InvokeAsync<DeductionEvaluationResult>(
            "evaluate",
            input,
            cancellationToken);
    }
}
