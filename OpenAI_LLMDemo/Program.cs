using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI_LLMDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Read the LLM settings from appsettings.json
string endpoint = builder.Configuration["LLM:Endpoint"] ?? "http://localhost:11434/v1";
string model = builder.Configuration["LLM:Model"] ?? "llama3.2";
string embeddingModel = builder.Configuration["LLM:EmbeddingModel"] ?? "nomic-embed-text";
string apiKey = builder.Configuration["LLM:ApiKey"] ?? "ollama"; // Ollama ignores the key, but it can't be empty

// 2. Create the AI clients once for the whole app. Ollama speaks the OpenAI API, so the OpenAI clients work with it.
var clientOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(endpoint),
    NetworkTimeout = TimeSpan.FromMinutes(5) // local models on a CPU are slow
};
builder.Services.AddSingleton(new ChatClient(model, new ApiKeyCredential(apiKey), clientOptions));                 // answers questions
builder.Services.AddSingleton(new EmbeddingClient(embeddingModel, new ApiKeyCredential(apiKey), clientOptions));   // turns text into vectors (RAG)

// 3. Our own services
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton<RagService>(); // singleton so the document vectors are calculated only once

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
