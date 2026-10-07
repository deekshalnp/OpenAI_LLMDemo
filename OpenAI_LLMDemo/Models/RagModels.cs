namespace OpenAI_LLMDemo.Models;

public class EmbedRequest
{
    public string Text { get; set; } = "";
}

// Step 1: what an embedding looks like
public record EmbedResponse(string Text, int Dimensions, float[] First10Numbers);

// Step 2: one search hit. Score 1 = same meaning, 0 = unrelated.
public record SearchResult(string Text, double Score);

// Step 3: the AI answer plus the documents it was given
public record RagResponse(string Answer, List<SearchResult> Sources);

// One document stored in our in-memory "vector database"
public record StoredDocument(string Text, float[] Vector);
