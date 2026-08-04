using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

[Serializable]
public class EvidenceRecord
{
    public List<EvidenceData> evidences;
    public string playerDescription;
}

[Serializable]
public class FinalDeductionInput
{
    public List<string> backgroundFacts;
    public List<EvidenceRecord> evidenceRecords;
}

[Serializable]
public class FinalDeductionResult
{
    public string narrative;
    public string culprit;
    public string method;
    public string motive;

    public List<string> reasoningPoints;

    public override string ToString()
    {
        string reasoningPointsText =
            reasoningPoints == null || reasoningPoints.Count == 0
                ? "없음"
                : string.Join("\n- ", reasoningPoints);
        
        return $"대사: {narrative}\n범인: {culprit}\n수법: {method}\n동기: {motive}\n추론포인트: {reasoningPointsText}";
    }
}

public class DetectiveAI
{
    private readonly AIEdgeFunctionClient client;

    public DetectiveAI(AIEdgeFunctionClient client)
    {
        this.client = client;
    }

    public UniTask<FinalDeductionResult> DeduceAsync(
        FinalDeductionInput input,
        CancellationToken cancellationToken = default)
    {
        return client.InvokeAsync<FinalDeductionResult>(
            "deduce",
            input,
            cancellationToken);
    }
}

