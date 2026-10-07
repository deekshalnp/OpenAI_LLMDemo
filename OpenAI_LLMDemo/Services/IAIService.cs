using OpenAI_LLMDemo.Models;

namespace OpenAI_LLMDemo.Services;

public interface IAIService
{
    Task<string> AskAsync(string question);

    Task<TechExplanation> ExplainAsync(string question);

    Task<ToolsResponse> AskWithToolsAsync(string question);
}
