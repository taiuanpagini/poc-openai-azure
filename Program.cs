using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Json;

namespace PocOpenAI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000);
            });

            // --- Config ---
            var config = builder.Configuration.GetSection("AzureOpenAI");
            string endpoint = config["Endpoint"] ?? throw new ArgumentNullException("Endpoint");
            string deployment = config["DeploymentName"] ?? throw new ArgumentNullException("DeploymentName");
            string? apiKey = config["ApiKey"];
            string batchUploadUrl = $"https://api.openai.com/v1/files";
            string batchCreateUrl = "https://api.openai.com/v1/batches";
            string apiVersion = "2024-02-15-preview";

            string url = $"openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            AzureOpenAIClient openAIClient = apiKey is not null
                ? new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey))
                : new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());

            ChatClient chat = openAIClient.GetChatClient(deployment);

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            builder.Services.AddSingleton(chat);

            var app = builder.Build();

            // DTOs
            app.MapPost("/extract", async (ExtractRequest req, ChatClient chat) =>
            {
                string batchText = string.Join("\n", req.Items);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("Analisar cada item listado abaixo para categorizar e extrair entidades de medicamentos/materiais. Responda SOMENTE no JSON do schema para cada item, mantendo a ordem."),
                    new UserChatMessage(batchText)
                };

                string schemaJson = """
                {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "brand": { "type": ["string","null"] },
                      "concentration": { "type": ["string","null"] },
                      "unit": { "type": ["string","null"], "description": "Unit não pode conter números" },
                      "form": { "type": ["string","null"], "description": "Ex: Ampola, Bolsa, Comprimido" },
                      "quantity": { "type": ["string","null"], "description": "Ex: 4ML, 100ML, 40 unidades" },
                      "category": { "type": ["string","null"] },
                      "additionalInfo": { "type": ["string","null"], "description": "Qualquer outra informação relevante" }
                    },
                    "additionalProperties": false
                }
                """;

                var options = new ChatCompletionOptions
                {
                    Temperature = 0,
                    //MaxOutputTokenCount = 2000,
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        "med_item",
                        BinaryData.FromString(schemaJson))
                };

                var result = await chat.CompleteChatAsync(messages, options);
                string json = result.Value.Content[0].Text;

                Console.WriteLine($"Tokens de entrada: {result.Value.Usage.InputTokenCount}");
                Console.WriteLine($"Tokens de saída: {result.Value.Usage.OutputTokenCount}");
                Console.WriteLine($"Total de tokens: {result.Value.Usage.TotalTokenCount}");

                return Results.Content(json, "application/json");
            });

            app.MapPost("extractBatch", async (ExtractRequest req) =>
            {
                var items = req.Items;
                var listItems = new List<object>();

                var sb = new StringBuilder();

                foreach (var item in items)
                {
                    var request = new
                    {
                        custom_id = "req-1",
                        method = "POST",
                        url = "/v1/chat/completions",
                        body = new
                        {
                            model = "gpt-4o-mini",
                            messages = new[] {
                                new { role = "system", content = "Você é um categorizador de materiais e deve analisar o item listado abaixo para categorizar e extrair entidades de medicamentos/materiais. Responda SOMENTE no JSON do schema para cada item, mantendo a ordem." },
                                new { role = "user", content = item }
                            },
                            max_tokens = 200,
                            tools = new[] {
                                new {
                                    type = "function",
                                    function = new {
                                        name = "categorizadorItens",
                                        description = "Categorizar itens de medicamento",
                                        parameters = new {
                                            type = "object",
                                            properties = new {
                                                name = new { type = "string" },
                                                //brand = new { type = "string" },
                                                //concentration = new { type = "string" },
                                                //unit = new { type = "string", description = "Unit não pode conter números" },
                                                //form = new { type = "string", description = "Ex: Ampola, Bolsa, Comprimido" },
                                                //quantity = new { type = "string", description = "Ex: 4ML, 100ML, 40 unidades" },
                                                //category = new { type = "string" },
                                                //additionalInfo = new { type = "string", description = "Qualquer outra informação relevante" }
                                            },
                                            required = new[] { "name", "unit" }
                                        }
                                    }
                                }
                            },
                            tool_choice = new {
                                type = "function",
                                function = new {
                                    name = "categorizadorItens"
                                }
                            }
                        }
                    };

                    sb.AppendLine(JsonSerializer.Serialize(request));
                }

                //var ndjsonBytes = Encoding.UTF8.GetBytes(sb.ToString());

                //using var form = new MultipartFormDataContent
                //{
                //    { new ByteArrayContent(ndjsonBytes), "file", "batch.ndjson" },
                //    { new StringContent("jsonl"), "purpose" }
                //};

                //var uploadResponse = await httpClient.PostAsync(batchUploadUrl, form);
                //if (!uploadResponse.IsSuccessStatusCode)
                //    return Results.Problem("Erro no upload: " + await uploadResponse.Content.ReadAsStringAsync());

                //var uploadResult = JsonDocument.Parse(await uploadResponse.Content.ReadAsStringAsync());
                //var fileId = uploadResult.RootElement.GetProperty("id").GetString();

                //// 3. Criar o batch
                //var batchBody = new
                //{
                //    input_file_id = fileId,
                //    endpoint = "/v1/chat/completions",
                //    completion_window = "24h"
                //};

                //var batchResponse = await httpClient.PostAsync(
                //    batchCreateUrl,
                //    new StringContent(JsonSerializer.Serialize(batchBody), Encoding.UTF8, "application/json")
                //);

                //if (!batchResponse.IsSuccessStatusCode)
                //    return Results.Problem("Erro ao criar batch: " + await batchResponse.Content.ReadAsStringAsync());

                //var batchResult = await batchResponse.Content.ReadAsStringAsync();
                //return Results.Ok(batchResult);

                var json = JsonSerializer.Serialize(sb);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("Resposta:");
                //Console.WriteLine(responseBody);

                return Results.Ok(responseBody);
            });

            app.MapPost("extractBatchAzure", async (ExtractRequest req, ChatClient chat) => {
                string schemaJson = """
                {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "brand": { "type": ["string","null"] },
                      "concentration": { "type": ["string","null"] },
                      "size": { "type": ["string","null"] },
                      "unit": { "type": ["string","null"], "description": "Unit não pode conter números" },
                      "form": { "type": ["string","null"], "description": "Ex: Ampola, Bolsa, Comprimido" },
                      "quantity": { "type": ["string","null"], "description": "Ex: 4ML, 100ML, 40 unidades" },
                      "category": { "type": ["string","null"] },
                      "additionalInfo": { "type": ["string","null"], "description": "Qualquer outra informação relevante" }
                    },
                    "additionalProperties": false
                }
                """;

                int totalInputTokens = 0;
                int totalOutputTokens = 0;

                var tasks = req.Items.Select(async item =>
                {
                    var messages = new List<ChatMessage>
                    {
                        new SystemChatMessage("Analise o item recebido para categorizar e extrair entidades de medicamentos/materiais. Responda SOMENTE no JSON do schema."),
                        new UserChatMessage(item)
                    };

                    var options = new ChatCompletionOptions
                    {
                        Temperature = 0,
                        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                            "med_item",
                            BinaryData.FromString(schemaJson))
                    };

                    var result = await chat.CompleteChatAsync(messages, options);

                    Interlocked.Add(ref totalInputTokens, result.Value.Usage.InputTokenCount);
                    Interlocked.Add(ref totalOutputTokens, result.Value.Usage.OutputTokenCount);

                    var json = JsonSerializer.Deserialize<object>(result.Value.Content[0].Text); 

                    return json;
                });

                var results = await Task.WhenAll(tasks);

                Console.WriteLine("Input tokens: ", totalInputTokens);
                Console.WriteLine("Output tokens: ", totalOutputTokens);

                return Results.Ok(new
                {
                    result = results,
                    inputTokens = totalInputTokens,
                    outputTokens = totalOutputTokens
                });
            });

            app.Run();
        }
}

public record ExtractRequest(List<string> Items);
public record MedItem(string name, string? concentration, string? unit, string? brand);
}
