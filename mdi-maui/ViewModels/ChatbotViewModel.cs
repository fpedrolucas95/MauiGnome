using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mdi_maui.Models;
using mdi_maui.Services;
using System.Collections.ObjectModel;

namespace mdi_maui.ViewModels;

public partial class ChatbotViewModel : ObservableObject
{
    #region Fields
    private readonly OpenAIService _openAIService;
    private readonly List<ChatMessage> _conversationContext;
    #endregion

    #region Properties

    [ObservableProperty]
    private string inputMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> messages = new();

    public bool IsEmpty => Messages.Count == 0;

    #endregion

    #region Constructor

    public ChatbotViewModel()
    {
        _openAIService = new OpenAIService();
        _conversationContext = new List<ChatMessage>();

        Messages.CollectionChanged += (_, __) => OnPropertyChanged(nameof(IsEmpty));
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputMessage)) return;

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = InputMessage,
            Timestamp = DateTime.Now
        };

        _conversationContext.Add(userMessage);
        Messages.Add(userMessage);

        try
        {
            var botResponse = await _openAIService.GetChatbotResponseAsync(_conversationContext);
            var botMessage = new ChatMessage
            {
                Role = "assistant",
                Content = botResponse,
                Timestamp = DateTime.Now
            };

            _conversationContext.Add(botMessage);
            Messages.Add(botMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"Erro: Não foi possível obter uma resposta do chatbot. {ex.Message}",
                Timestamp = DateTime.Now
            };

            _conversationContext.Add(errorMessage);
            Messages.Add(errorMessage);
        }

        InputMessage = string.Empty;
    }

    #endregion
}