using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public sealed class SupabaseAnonymousAuthClient
{
    private const string AccessTokenKey =
        "DetectiveAI.Supabase.AccessToken";
    private const string RefreshTokenKey =
        "DetectiveAI.Supabase.RefreshToken";
    private const string ExpiresAtKey =
        "DetectiveAI.Supabase.ExpiresAt";
    private const long RefreshMarginSeconds = 60;

    private readonly string projectUrl;
    private readonly string publishableKey;
    private readonly SemaphoreSlim sessionGate = new SemaphoreSlim(1, 1);

    public SupabaseAnonymousAuthClient(
        string supabaseProjectUrl,
        string publishableKey)
    {
        projectUrl = supabaseProjectUrl.TrimEnd('/');
        this.publishableKey = publishableKey;
    }

    public async UniTask<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        await sessionGate.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableAccessToken())
                return PlayerPrefs.GetString(AccessTokenKey);

            string refreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                AuthSession refreshed = await RequestSessionAsync(
                    $"{projectUrl}/auth/v1/token?grant_type=refresh_token",
                    new JObject { ["refresh_token"] = refreshToken },
                    cancellationToken,
                    allowAuthFailure: true);

                if (refreshed != null)
                {
                    SaveSession(refreshed);
                    return refreshed.access_token;
                }

                ClearSession();
            }

            AuthSession created = await RequestSessionAsync(
                $"{projectUrl}/auth/v1/signup",
                new JObject(),
                cancellationToken,
                allowAuthFailure: false);

            SaveSession(created);
            return created.access_token;
        }
        finally
        {
            sessionGate.Release();
        }
    }

    public void InvalidateAccessToken()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(ExpiresAtKey);
        PlayerPrefs.Save();
    }

    private bool HasUsableAccessToken()
    {
        string accessToken = PlayerPrefs.GetString(AccessTokenKey, "");
        string expiresAtText = PlayerPrefs.GetString(ExpiresAtKey, "0");

        return !string.IsNullOrWhiteSpace(accessToken) &&
               long.TryParse(expiresAtText, out long expiresAt) &&
               expiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
                   RefreshMarginSeconds;
    }

    private async UniTask<AuthSession> RequestSessionAsync(
        string url,
        JObject body,
        CancellationToken cancellationToken,
        bool allowAuthFailure)
    {
        using var request = new UnityWebRequest(
            url,
            UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(
            Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", publishableKey);

        try
        {
            await request.SendWebRequest()
                .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        string responseBody = request.downloadHandler?.text;
        if (request.result != UnityWebRequest.Result.Success)
        {
            if (allowAuthFailure && request.responseCode >= 400 &&
                request.responseCode < 500)
            {
                return null;
            }

            throw new SupabaseAuthException(
                request.responseCode,
                request.error,
                responseBody);
        }

        AuthSession session =
            JsonConvert.DeserializeObject<AuthSession>(responseBody);

        if (session == null ||
            string.IsNullOrWhiteSpace(session.access_token) ||
            string.IsNullOrWhiteSpace(session.refresh_token))
        {
            throw new InvalidOperationException(
                "Supabase Auth response did not contain a valid session.");
        }

        return session;
    }

    private static void SaveSession(AuthSession session)
    {
        long expiresAt = session.expires_at > 0
            ? session.expires_at
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
              Math.Max(session.expires_in, 60);

        PlayerPrefs.SetString(AccessTokenKey, session.access_token);
        PlayerPrefs.SetString(RefreshTokenKey, session.refresh_token);
        PlayerPrefs.SetString(ExpiresAtKey, expiresAt.ToString());
        PlayerPrefs.Save();
    }

    private static void ClearSession()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey(ExpiresAtKey);
        PlayerPrefs.Save();
    }

    [Serializable]
    private sealed class AuthSession
    {
        public string access_token;
        public string refresh_token;
        public long expires_in;
        public long expires_at;
    }
}

public sealed class SupabaseAuthException : Exception
{
    public long StatusCode { get; }
    public string ResponseBody { get; }

    public SupabaseAuthException(
        long statusCode,
        string requestError,
        string responseBody)
        : base(
            $"Supabase anonymous authentication failed. " +
            $"Status: {statusCode}, Error: {requestError}, " +
            $"Response: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}