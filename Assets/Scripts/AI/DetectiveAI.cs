using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
    private readonly OpenAIClient client;
    private readonly string DetectiveInstructions;

    public DetectiveAI(
        OpenAIClient client,
        string detectiveInstructions)
    {
        this.client = client;
        this.DetectiveInstructions = detectiveInstructions;
    }

    public async UniTask<FinalDeductionResult> DeduceAsync(
        FinalDeductionInput input,
        CancellationToken cancellationToken = default)
    {
        string requestJson =
            OpenAIRequestBuilder.BuildStructuredRequest(
                "gpt-5.4-mini",
                DetectiveInstructions,
                input,
                effort: "low",
                max_output_tokens: 800,
                verbosity: "medium",
                "final_deduction_result",
                CreateFinalDeductionSchema());

        string responseJson =
            await client.SendAsync(
                requestJson,
                cancellationToken);

        return OpenAIResponseParser
            .ParseStructuredOutput<FinalDeductionResult>(
                responseJson);
    }
    
    public static JObject CreateFinalDeductionSchema()
    {
        return new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["narrative"] = StringSchema(),
                ["culprit"] = StringSchema(),
                ["method"] = StringSchema(),
                ["motive"] = StringSchema(),
                ["reasoningPoints"] = new JObject
                {
                    ["type"] = "array",
                    ["items"] = new JObject
                    {
                        ["type"] = "string"
                    }
                }
            },
            ["required"] = new JArray(
                "narrative",
                "culprit",
                "method",
                "motive",
                "reasoningPoints"),
            ["additionalProperties"] = false
        };
    }

    private static JObject StringSchema()
    {
        return new JObject
        {
            ["type"] = "string"
        };
    }

    private static JObject StringArraySchema()
    {
        return new JObject
        {
            ["type"] = "array",
            ["items"] = StringSchema()
        };
    }
}

