using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI_LLMDemo.Data;
using OpenAI_LLMDemo.Models;
using OpenAI_LLMDemo.Prompts;

namespace OpenAI_LLMDemo.Services;

public class RagService(EmbeddingClient embeddingClient, ChatClient chatClient)
{
    // Our "vector database": every document together with its embedding, kept in memory.
    private List<StoredDocument>? _store;

    // ---------- STEP 1: Embedding = turn text into numbers that represent its meaning ----------
    public async Task<float[]> EmbedAsync(string text)
    {
        OpenAIEmbedding embedding = await embeddingClient.GenerateEmbeddingAsync(text);
        return embedding.ToFloats().ToArray();
    }

    // ---------- STEP 2: Vector search = find the documents with the most similar meaning ----------
    public async Task<List<SearchResult>> SearchAsync(string question, int top = 3)
    {
        List<StoredDocument> store = await GetStoreAsync();
        float[] questionVector = await EmbedAsync(question);

        return store
            .Select(doc => new SearchResult(doc.Text, CosineSimilarity(questionVector, doc.Vector)))
            .OrderByDescending(result => result.Score)
            .Take(top)
            .ToList();
    }

    // ---------- STEP 3: RAG = give the found documents to the AI so it can answer ----------
    public async Task<RagResponse> AskAsync(string question)
    {
        // R = Retrieve
        List<SearchResult> sources = await SearchAsync(question);

        // A = Augment (put the documents into the prompt)
        string information = string.Join("\n", sources.Select(source => "- " + source.Text));
        List<ChatMessage> messages =
        [
            new SystemChatMessage(PromptTemplates.RagPrompt),
            new UserChatMessage($"Information:\n{information}\n\nQuestion: {question}")
        ];

        // G = Generate
        ChatCompletion completion = await chatClient.CompleteChatAsync(messages);
        string answer = completion.Content.Count > 0 ? completion.Content[0].Text : "";

        return new RagResponse(answer, sources);
    }

    // Embed all documents once (the first time someone searches), then reuse them.
    private async Task<List<StoredDocument>> GetStoreAsync()
    {
        if (_store is null)
        {
            var store = new List<StoredDocument>();
            foreach (string document in KnowledgeBase.Documents)
            {
                store.Add(new StoredDocument(document, await EmbedAsync(document)));
            }
            _store = store;
        }

        return _store;
    }

    // How close two vectors point in the same direction: 1 = same meaning, 0 = unrelated.
    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, lengthA = 0, lengthB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }

        return Math.Round(dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB)), 3);
    }
}
