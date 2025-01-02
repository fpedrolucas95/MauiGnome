using System.Text.Json.Serialization;

namespace mdi_maui.Models;

public class ChatMessage
{
    public ChatMessage()
    {
        Role = string.Empty;
        Content = string.Empty;
    }

    public string Role { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public bool IsUserMessage => Role?.ToLower() == "user";
}

public class ChatCompletionModel
{
    public ChatCompletionModel()
    {
        Id = string.Empty;
        Object = string.Empty;
        Model = string.Empty;
        Choices = new List<Choice>();
        Usage = new Usage();
        SystemFingerprint = string.Empty;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; }

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; }
}

public class Choice
{
    public Choice()
    {
        Message = new Message();
        Logprobs = new object();
        FinishReason = string.Empty;
    }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("logprobs")]
    public object Logprobs { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class Message
{
    public Message()
    {
        Role = string.Empty;
        Content = string.Empty;
        Refusal = string.Empty;
    }

    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("refusal")]
    public string Refusal { get; set; }
}

public class Usage
{
    public Usage()
    {
        PromptTokensDetails = new PromptTokensDetails();
        CompletionTokensDetails = new CompletionTokensDetails();
    }

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails PromptTokensDetails { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails CompletionTokensDetails { get; set; }
}

public class PromptTokensDetails
{
    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }
}

public class CompletionTokensDetails
{
    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonPropertyName("accepted_prediction_tokens")]
    public int AcceptedPredictionTokens { get; set; }

    [JsonPropertyName("rejected_prediction_tokens")]
    public int RejectedPredictionTokens { get; set; }
}
