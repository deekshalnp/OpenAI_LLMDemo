using System.Text.Json;
using OpenAI.Chat;
using OpenAI_LLMDemo.Models;
using OpenAI_LLMDemo.Prompts;

namespace OpenAI_LLMDemo.Services;

public class AIService(ChatClient chatClient, WeatherService weatherService) : IAIService
{
    // --- Simple question -> text answer ----
    public async Task<string> AskAsync(string question)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(PromptTemplates.AskPrompt),
            new UserChatMessage(question)
        ];

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages);
        return GetText(completion);
    }

    // --- Structured output -> JSON that matches a schema -------
    public async Task<TechExplanation> ExplainAsync(string question)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(PromptTemplates.ExplainPrompt),
            new UserChatMessage(question)
        ];

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "tech_explanation",
                BinaryData.FromString(PromptTemplates.ExplainJsonSchema),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
        string json = GetText(completion);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<TechExplanation>(json, jsonOptions)
               ?? throw new Exception("The AI returned empty JSON.");
    }

    // ----Tool calling -> the model asks us to run C# functions -----
    private static readonly BinaryData CityParameter = BinaryData.FromString(
        """
        {
          "type": "object",
          "properties": {
            "city": { "type": "string", "description": "City name, for example London" }
          },
          "required": ["city"]
        }
        """);

    private static readonly ChatTool WeatherTool = ChatTool.CreateFunctionTool(
        "get_weather", "Get the current weather for a city.", CityParameter);

    private static readonly ChatTool TimeTool = ChatTool.CreateFunctionTool(
        "get_time", "Get the current local time for a city.", CityParameter);

    public async Task<ToolsResponse> AskWithToolsAsync(string question)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(PromptTemplates.ToolsPrompt),
            new UserChatMessage(question)
        ];

        var options = new ChatCompletionOptions();
        options.Tools.Add(WeatherTool);
        options.Tools.Add(TimeTool);

        var toolsUsed = new List<string>();

        // Max 5 rounds so we never loop forever.
        for (int round = 1; round <= 5; round++)
        {
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

            // The model did not ask for a tool -> this is the final answer.
            if (completion.ToolCalls.Count == 0)
            {
                return new ToolsResponse(GetText(completion), toolsUsed);
            }

            // Remember what the model asked for.
            messages.Add(new AssistantChatMessage(completion));

            // Run each tool and send the result back to the model.
            foreach (ChatToolCall toolCall in completion.ToolCalls)
            {
                string city = GetCity(toolCall);
                string result = await RunToolAsync(toolCall.FunctionName, city);

                messages.Add(new ToolChatMessage(toolCall.Id, result));
                toolsUsed.Add($"{toolCall.FunctionName}({city}) = {result}");
            }
        }

        throw new Exception("The AI kept calling tools and never gave an answer.");
    }

    private async Task<string> RunToolAsync(string toolName, string city)
    {
        return toolName switch
        {
            "get_weather" => await weatherService.GetWeatherAsync(city),
            "get_time" => await weatherService.GetTimeAsync(city),
            _ => $"Unknown tool: {toolName}"
        };
    }

    // The model sends the arguments as JSON, for example: { "city": "London" }
    private static string GetCity(ChatToolCall toolCall)
    {
        using JsonDocument args = JsonDocument.Parse(toolCall.FunctionArguments);
        return args.RootElement.GetProperty("city").GetString() ?? "";
    }

    private static string GetText(ChatCompletion completion) =>
        completion.Content.Count > 0 ? completion.Content[0].Text : "";
}
