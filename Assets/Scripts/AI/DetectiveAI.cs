using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

[Serializable]
public class FinalDeductionInput
{
    public List<string> backgroundFacts;
    public List<EvidenceData> evidences;
    public List<string> hypotheses;
}

[Serializable]
public class FinalDeductionResult
{
    public string narrative;
    public string culprit;
    public string method;
    public string motive;
}

public class DetectiveAI
{
    private readonly OpenAIClient client;
    private readonly string model;
    private readonly string DetectiveInstructions;

    public DetectiveAI(
        OpenAIClient client,
        string model, string detectiveInstructions)
    {
        this.client = client;
        this.model = model;
        this.DetectiveInstructions = detectiveInstructions;
    }

    public async UniTask<FinalDeductionResult> DeduceAsync(
        FinalDeductionInput input,
        CancellationToken cancellationToken = default)
    {
        string requestJson =
            OpenAIRequestBuilder.BuildStructuredRequest(
                model,
                DetectiveInstructions,
                input,
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
            },
            ["required"] = new JArray(
                "narrative",
                "culprit",
                "method",
                "motive"),
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

