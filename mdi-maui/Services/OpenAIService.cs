using System.Text;
using mdi_maui.Models;
using System.Text.Json;

namespace mdi_maui.Services;

public class OpenAIService
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string ApiKey = "YOUR_API_KEY";

    private readonly HttpClient _httpClient;

    public OpenAIService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
    }

    public async Task<string> GetChatbotResponseAsync(List<ChatMessage> conversationContext)
    {
        try
        {
            var requestPayload = new
            {
                model = "gpt-4o-mini",
                messages = conversationContext.Select(m => new { role = m.Role, content = m.Content }).ToList()
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar a API OpenAI: {response.ReasonPhrase}. Verifique o arquivo OpenAPIService.cs em Services, e insira a sua chave de API em ApiKey");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Resposta JSON: {responseContent}");

            var result = JsonSerializer.Deserialize<ChatCompletionModel>(responseContent);

            if (result == null)
            {
                throw new Exception("Resposta da API é nula.");
            }

            if (result.Choices == null || result.Choices.Count == 0)
            {
                throw new Exception("Nenhuma escolha encontrada na resposta da API.");
            }

            var botResponse = result.Choices.First().Message?.Content;

            if (string.IsNullOrEmpty(botResponse))
            {
                throw new Exception("A resposta do assistente é nula ou vazia.");
            }

            return botResponse;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Exception: {ex.Message}");
            throw new Exception($"Erro ao processar o JSON retornado: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw new Exception($"Erro na solicitação para a API: {ex.Message}");
        }
    }
}