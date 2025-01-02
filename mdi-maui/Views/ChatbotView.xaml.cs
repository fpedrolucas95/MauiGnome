using mdi_maui.ViewModels;

namespace mdi_maui.Views
{
    public partial class ChatbotView : ContentView
    {
        public ChatbotView()
        {
            InitializeComponent();
            BindingContext = new ChatbotViewModel();
        }
    }
}
