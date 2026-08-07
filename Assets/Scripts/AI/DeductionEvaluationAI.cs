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
    public float motiveScore;
    public float sceneScore;
    public float timeScore;
    public float accessMethodScore;
    public float coreActionScore;
    public float originalStatusScore;
    public float copyDestinationScore;
    public float tasteGapReasonScore;
    public List<string> detectedMisleadingClaims;
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
