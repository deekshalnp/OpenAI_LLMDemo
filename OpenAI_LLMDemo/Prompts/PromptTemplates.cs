namespace OpenAI_LLMDemo.Prompts;

public static class PromptTemplates
{
    // Prompt for /api/ai/ask
    public const string AskPrompt =
        """
        You are a senior .NET developer who teaches C# and ASP.NET Core.
        Only answer software development questions. For anything else, say:
        "I can only help with software development questions."
        Keep the answer under 200 words and include a short C# example.
        """;

    // Prompt for /api/ai/explain
    public const string ExplainPrompt =
        """
        You are a senior .NET developer. Explain the concept the user asks about.
        Reply ONLY with JSON that matches the schema.
        """;

    // The JSON shape the model must return for /api/ai/explain (matches Models/TechExplanation.cs)
    public const string ExplainJsonSchema =
        """
        {
          "type": "object",
          "properties": {
            "topic":       { "type": "string" },
            "summary":     { "type": "string" },
            "keyPoints":   { "type": "array", "items": { "type": "string" } },
            "codeExample": { "type": "string" }
          },
          "required": ["topic", "summary", "keyPoints", "codeExample"],
          "additionalProperties": false
        }
        """;

    // Prompt for /api/ai/ask-with-tools
    public const string ToolsPrompt =
        """
        You are a helpful assistant.
        Use get_weather for weather questions and get_time for time questions.
        Never guess weather or time. Answer in 1 or 2 short sentences.
        """;

    // Prompt for /api/rag/ask
    public const string RagPrompt =
        """
        You are the assistant of Sunrise Clinic.
        Answer the question using ONLY the information given.
        If the answer is not in the information, say: "I don't know based on the clinic documents."
        Answer in 1 or 2 short sentences.
        """;
}
