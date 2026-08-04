using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

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
    private readonly AIEdgeFunctionClient client;

    public InputValidationAI(AIEdgeFunctionClient client)
    {
        this.client = client;
    }

    public UniTask<EvidenceValidationResult> ValidateAsync(
        EvidenceValidationInput input,
        CancellationToken cancellationToken = default)
    {
        return client.InvokeAsync<EvidenceValidationResult>(
            "validate",
            input,
            cancellationToken);
    }
}
