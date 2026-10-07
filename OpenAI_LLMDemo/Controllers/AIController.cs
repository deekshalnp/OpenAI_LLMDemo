using Microsoft.AspNetCore.Mvc;
using OpenAI_LLMDemo.Models;
using OpenAI_LLMDemo.Services;

namespace OpenAI_LLMDemo.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController(IAIService aiService) : ControllerBase
{
    // POST api/ai/ask  ->  { "answer": "..." }
    [HttpPost("ask")]
    public async Task<IActionResult> Ask(AskRequest request)
    {
        try
        {
            string answer = await aiService.AskAsync(request.Question);
            return Ok(new AskResponse(answer));
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    // POST api/ai/explain  ->  structured JSON (topic, summary, keyPoints, codeExample)
    [HttpPost("explain")]
    public async Task<IActionResult> Explain(AskRequest request)
    {
        try
        {
            TechExplanation explanation = await aiService.ExplainAsync(request.Question);
            return Ok(explanation);
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    // POST api/ai/ask-with-tools  ->  the model can call our weather/time tools
    [HttpPost("ask-with-tools")]
    public async Task<IActionResult> AskWithTools(AskRequest request)
    {
        try
        {
            ToolsResponse response = await aiService.AskWithToolsAsync(request.Question);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    private ObjectResult AiError(Exception ex) =>
        Problem(
            title: "The AI request failed. Is Ollama running? (ollama serve)",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
}
