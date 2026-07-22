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

    public override string ToString()
    {
        return $"상태: {status}\n대사: {response}";
    }
}

public class InputValidationAI
{
    private readonly OpenAIClient client;
    
    private readonly string InputValidationInstructions;

    public InputValidationAI(
        OpenAIClient client,
        string inputValidationInstructions)
    {
        this.client = client;
        this.InputValidationInstructions = inputValidationInstructions;
    }

    public async UniTask<EvidenceValidationResult> ValidateAsync(
        EvidenceValidationInput input,
        CancellationToken cancellationToken = default)
    {
        string requestJson =
            OpenAIRequestBuilder.BuildStructuredRequest(
                model: "gpt-5.4-nano",
                instructions: InputValidationInstructions,
                inputData: input,
                effort: "none",
                max_output_tokens: 300,
                verbosity: "low",
                schemaName: "evidence_validation_result",
                schema: CreateValidationSchema());
        
        string responseJson =
            await client.SendAsync(
                requestJson,
                cancellationToken);
        
        var r = OpenAIResponseParser
            .ParseStructuredOutput<EvidenceValidationResult>(
                responseJson);
        return r;
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
            },
            ["required"] = new JArray(
                "status",
                "response"),
            ["additionalProperties"] = false
        };
    }
}
