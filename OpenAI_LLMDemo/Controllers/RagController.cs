using Microsoft.AspNetCore.Mvc;
using OpenAI_LLMDemo.Data;
using OpenAI_LLMDemo.Models;
using OpenAI_LLMDemo.Services;

namespace OpenAI_LLMDemo.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController(RagService ragService) : ControllerBase
{
    // GET api/rag/documents  ->  the documents the AI can search
    [HttpGet("documents")]
    public IActionResult Documents() => Ok(KnowledgeBase.Documents);

    // STEP 1 - POST api/rag/embed  ->  see what an embedding looks like
    [HttpPost("embed")]
    public async Task<IActionResult> Embed(EmbedRequest request)
    {
        try
        {
            float[] vector = await ragService.EmbedAsync(request.Text);
            return Ok(new EmbedResponse(request.Text, vector.Length, vector.Take(10).ToArray()));
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    // STEP 2 - POST api/rag/search  ->  the 3 documents closest in meaning to the question
    [HttpPost("search")]
    public async Task<IActionResult> Search(AskRequest request)
    {
        try
        {
            List<SearchResult> results = await ragService.SearchAsync(request.Question);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    // STEP 3 - POST api/rag/ask  ->  search + give the results to the AI + answer
    [HttpPost("ask")]
    public async Task<IActionResult> Ask(AskRequest request)
    {
        try
        {
            RagResponse response = await ragService.AskAsync(request.Question);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return AiError(ex);
        }
    }

    private ObjectResult AiError(Exception ex) =>
        Problem(
            title: "The AI request failed. Is Ollama running, and is the embedding model installed? (ollama pull nomic-embed-text)",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
}
