using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OpenAIResponse
{
    public string id;
    public string status;
    public List<OpenAIOutputItem> output;
    public OpenAIError error;
}

[Serializable]
public class OpenAIOutputItem
{
    public string type;
    public string role;
    public List<OpenAIContentItem> content;
}

[Serializable]
public class OpenAIContentItem
{
    public string type;
    public string text;
}

[Serializable]
public class OpenAIError
{
    public string message;
    public string type;
    public string code;
}

public class OpenAIResponseParser
{
    public static string ExtractOutputText(string responseJson)
    {
        OpenAIResponse response =
            JsonUtility.FromJson<OpenAIResponse>(responseJson);

        if (response == null)
            throw new InvalidOperationException(
                "Failed to parse OpenAI response.");

        if (response.status != "completed")
        {
            string errorMessage =
                response.error?.message ?? "Unknown response error.";

            throw new InvalidOperationException(
                $"OpenAI response was not completed: {errorMessage}");
        }

        if (response.output == null)
            throw new InvalidOperationException(
                "The response contains no output.");

        foreach (OpenAIOutputItem outputItem in response.output)
        {
            if (outputItem.type != "message" ||
                outputItem.content == null)
            {
                continue;
            }

            foreach (OpenAIContentItem contentItem in outputItem.content)
            {
                if (contentItem.type == "output_text")
                    return contentItem.text;
            }
        }

        throw new InvalidOperationException(
            "The response contains no output_text.");
    }

    public static T ParseStructuredOutput<T>(string responseJson)
    {
        string outputText = ExtractOutputText(responseJson);
        T result = JsonUtility.FromJson<T>(outputText);

        if (result == null)
        {
            throw new InvalidOperationException(
                $"Failed to parse structured output as {typeof(T).Name}.");
        }

        return result;
    }
}
