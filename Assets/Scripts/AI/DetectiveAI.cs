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
    public string motive;
    public string scene;
    public string time;
    public string accessMethod;
    public string coreAction;
    public string originalStatus;
    public string copyDestination;
    public string tasteGapReason;

    public override string ToString()
    {
        return $"대사: {narrative}\n범인: {culprit}\n동기: {motive}";
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

