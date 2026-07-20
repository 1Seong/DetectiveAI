using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OpenAIRequestBuilder
{
    public static string BuildStructuredRequest(
        string model,
        string instructions,
        object inputData,
        string schemaName,
        JObject schema)
    {
        var request = new JObject
        {
            ["model"] = model,
            ["instructions"] = instructions,

            // 게임 데이터를 JSON 문자열로 전달
            ["input"] = JsonConvert.SerializeObject(inputData),

            ["text"] = new JObject
            {
                ["format"] = new JObject
                {
                    ["type"] = "json_schema",
                    ["name"] = schemaName,
                    ["strict"] = true,
                    ["schema"] = schema
                }
            }
        };

        return request.ToString(Formatting.None);
    }
}
