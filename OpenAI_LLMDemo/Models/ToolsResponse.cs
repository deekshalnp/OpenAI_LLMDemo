namespace OpenAI_LLMDemo.Models;

public record ToolsResponse(string Answer, List<string> ToolsUsed);
