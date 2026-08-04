using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public sealed class AIEdgeFunctionClient
{
    private const string FunctionName = "detective-ai";

    private readonly string functionUrl;
    private readonly string publishableKey;
    private readonly SupabaseAnonymousAuthClient authClient;

    public AIEdgeFunctionClient(
        string supabaseProjectUrl,
        string publishableKey)
    {
        if (string.IsNullOrWhiteSpace(supabaseProjectUrl))
            throw new ArgumentException("Supabase project URL is empty.");

        if (string.IsNullOrWhiteSpace(publishableKey))
            throw new ArgumentException("Supabase publishable key is empty.");

        functionUrl =
            $"{supabaseProjectUrl.TrimEnd('/')}/functions/v1/{FunctionName}";
        this.publishableKey = publishableKey;
        authClient = new SupabaseAnonymousAuthClient(
            supabaseProjectUrl,
            publishableKey);
    }

    public async UniTask<T> InvokeAsync<T>(
        string action,
        object input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("AI action is empty.");

        JObject payload = new JObject
        {
            ["action"] = action,
            ["input"] = input == null
                ? JValue.CreateNull()
                : JToken.FromObject(input)
        };

        string requestBody = payload.ToString(Formatting.None);
        string accessToken =
            await authClient.GetAccessTokenAsync(cancellationToken);

        EdgeHttpResponse response = await SendAsync(
            requestBody,
            accessToken,
            cancellationToken);

        if (response.StatusCode == 401)
        {
            authClient.InvalidateAccessToken();
            accessToken = await authClient.GetAccessTokenAsync(cancellationToken);
            response = await SendAsync(
                requestBody,
                accessToken,
                cancellationToken);
        }

        if (!response.Success)
        {
            throw new AIEdgeFunctionException(
                response.StatusCode,
                response.RequestError,
                response.Body);
        }

        T result = JsonConvert.DeserializeObject<T>(response.Body);
        if (result == null)
        {
            throw new InvalidOperationException(
                $"Failed to parse Edge Function response as {typeof(T).Name}.");
        }

        return result;
    }

    private async UniTask<EdgeHttpResponse> SendAsync(
        string requestBody,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new UnityWebRequest(
            functionUrl,
            UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(
            Encoding.UTF8.GetBytes(requestBody));
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", publishableKey);
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        try
        {
            await request.SendWebRequest()
                .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        return new EdgeHttpResponse
        {
            Success = request.result == UnityWebRequest.Result.Success,
            StatusCode = request.responseCode,
            RequestError = request.error,
            Body = request.downloadHandler?.text
        };
    }

    private sealed class EdgeHttpResponse
    {
        public bool Success;
        public long StatusCode;
        public string RequestError;
        public string Body;
    }
}

public sealed class AIEdgeFunctionException : Exception
{
    public long StatusCode { get; }
    public string ErrorCode { get; }
    public string ResponseBody { get; }
    public int RetryAfterSeconds { get; }

    public AIEdgeFunctionException(
        long statusCode,
        string requestError,
        string responseBody)
        : base(CreateMessage(statusCode, requestError, responseBody))
    {
        StatusCode = statusCode;
        ErrorCode = ExtractErrorCode(responseBody);
        ResponseBody = responseBody;
        RetryAfterSeconds = ExtractRetryAfterSeconds(responseBody);
    }

    private static string CreateMessage(
        long statusCode,
        string requestError,
        string responseBody)
    {
        string serverMessage = ExtractServerMessage(responseBody);
        return
            $"AI Edge Function request failed. " +
            $"Status: {statusCode}, Error: {requestError}, " +
            $"Server: {serverMessage}";
    }

    private static string ExtractErrorCode(string responseBody)
    {
        try
        {
            return JObject.Parse(responseBody)["error"]?["code"]?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractServerMessage(string responseBody)
    {
        try
        {
            return JObject.Parse(responseBody)["error"]?["message"]?.ToString()
                   ?? "Unknown server error.";
        }
        catch
        {
            return string.IsNullOrWhiteSpace(responseBody)
                ? "Unknown server error."
                : responseBody;
        }
    }

    private static int ExtractRetryAfterSeconds(string responseBody)
    {
        try
        {
            return JObject.Parse(responseBody)["error"]?["retryAfterSeconds"]
                       ?.Value<int>() ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}
