using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class OpenAIClient
{
    private const string ResponsesUrl =
        "https://api.openai.com/v1/responses";

    private readonly string apiKey;

    public OpenAIClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("OpenAI API key is empty.");

        this.apiKey = apiKey;
    }

    public async UniTask<string> SendAsync(
        string requestJson,
        CancellationToken cancellationToken = default)
    {
        using var request = new UnityWebRequest(
            ResponsesUrl,
            UnityWebRequest.kHttpVerbPOST);

        byte[] body = Encoding.UTF8.GetBytes(requestJson);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader(
            "Authorization",
            $"Bearer {apiKey}");

        try
        {
            await request
                .SendWebRequest()
                .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new OpenAIRequestException(
                request.responseCode,
                request.error,
                request.downloadHandler?.text);
        }

        return request.downloadHandler.text;
    }
}

public sealed class OpenAIRequestException : Exception
{
    public long StatusCode { get; }
    public string ResponseBody { get; }

    public OpenAIRequestException(
        long statusCode,
        string requestError,
        string responseBody)
        : base(
            $"OpenAI request failed. " +
            $"Status: {statusCode}, Error: {requestError}, " +
            $"Body: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
