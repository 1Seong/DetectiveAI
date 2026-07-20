using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

[Serializable]
public class EvidenceValidationInput
{
    public List<EvidenceData> evidenceDatas;
    public string playerDescription;
}

public enum InputStatus
{
    Accept,
    RequestClarification
}

[Serializable]
public class EvidenceValidationResult
{
    public string status;
    public string response;
    public List<string> hypotheses;

    public InputStatus GetStatus()
    {
        return status switch
        {
            "Accept" => InputStatus.Accept,
            "RequestClarification" =>
                InputStatus.RequestClarification,
            _ => throw new InvalidOperationException(
                $"Unknown input status: {status}")
        };
    }
}

public class InputValidationAI
{
    private readonly OpenAIClient client;

    // Structured Outputs를 지원하는 모델명으로 설정
    private readonly string model;
    private readonly string InputValidationInstructions;

    public InputValidationAI(
        OpenAIClient client,
        string model,
        string inputValidationInstructions)
    {
        this.client = client;
        this.model = model;
        this.InputValidationInstructions = inputValidationInstructions;
    }

    public async UniTask<EvidenceValidationResult> ValidateAsync(
        EvidenceValidationInput input,
        CancellationToken cancellationToken = default)
    {
        string requestJson =
            OpenAIRequestBuilder.BuildStructuredRequest(
                model: model,
                instructions: InputValidationInstructions,
                inputData: input,
                schemaName: "evidence_validation_result",
                schema: CreateValidationSchema());

        string responseJson =
            await client.SendAsync(
                requestJson,
                cancellationToken);

        return OpenAIResponseParser
            .ParseStructuredOutput<EvidenceValidationResult>(
                responseJson);
    }
    
    public static JObject CreateValidationSchema()
    {
        return new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["status"] = new JObject
                {
                    ["type"] = "string",
                    ["enum"] = new JArray(
                        "Accept",
                        "RequestClarification")
                },
                ["response"] = new JObject
                {
                    ["type"] = "string"
                },
                ["hypotheses"] = new JObject
                {
                    ["type"] = "array",
                    ["items"] = new JObject
                    {
                        ["type"] = "string"
                    }
                }
            },
            ["required"] = new JArray(
                "status",
                "response",
                "hypotheses"),
            ["additionalProperties"] = false
        };
    }
}
