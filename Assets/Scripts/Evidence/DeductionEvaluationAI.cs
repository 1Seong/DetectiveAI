using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
    private readonly OpenAIClient client;
    private readonly string EvaluationInstruction;

    public DeductionEvaluationAI(
        OpenAIClient client,
        string evaluationInstruction)
    {
        this.client = client;
        this.EvaluationInstruction = evaluationInstruction;
    }

    public async UniTask<DeductionEvaluationResult> EvaluateAsync(
        DeductionEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        string requestJson =
            OpenAIRequestBuilder.BuildStructuredRequest(
                "gpt-5.4-mini",
                EvaluationInstruction,
                input,
                effort: "low",
                max_output_tokens: 500,
                verbosity: "low",
                "deduction_evaluation_result",
                CreateEvaluationSchema());

        string responseJson =
            await client.SendAsync(
                requestJson,
                cancellationToken);

        return OpenAIResponseParser
            .ParseStructuredOutput<DeductionEvaluationResult>(
                responseJson);
    }
    
    public static JObject CreateEvaluationSchema()
    {
        return new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["culpritScore"] = ScoreSchema(),
                ["methodScore"] = ScoreSchema(),
                ["motiveScore"] = ScoreSchema(),
                ["keyPointScore"] = ScoreSchema(),
                ["matchedPoints"] = StringArraySchema(),
                ["detectedMisleadingClaims"] = StringArraySchema()
            },
            ["required"] = new JArray(
                "culpritScore",
                "methodScore",
                "motiveScore",
                "keyPointScore",
                "matchedPoints",
                "detectedMisleadingClaims"),
            ["additionalProperties"] = false
        };
    }

    private static JObject ScoreSchema()
    {
        return new JObject
        {
            ["type"] = "number",
            ["minimum"] = 0.0,
            ["maximum"] = 1.0
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
