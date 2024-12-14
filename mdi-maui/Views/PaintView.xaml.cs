using mdi_maui.ViewModels;

namespace mdi_maui.Views
{
    public partial class PaintView : ContentView
    {
        private readonly PaintViewModel _viewModel;

        public PaintView()
        {
            InitializeComponent();
            _viewModel = new PaintViewModel();
            BindingContext = _viewModel;
        }

        private void OnColorSelection(object sender, EventArgs e)
        {
            if (sender is Button button && !string.IsNullOrWhiteSpace(button.Text))
            {
                _viewModel.ToolColor = Color.Parse(button.Text);
            }
        }

        private async void OnSaveButtonPressed(object sender, EventArgs e)
        {
            if (DrawingCanvas == null) return;

            try
            {
                using var stream = await DrawingCanvas.GetImageStream(800, 800);
                if (stream == null) return;

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Test.png");
                await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);

                await DisplayAlert("Sucesso", "Imagem salva com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar a imagem: {ex.Message}", "OK");
            }
        }

        private void ClearDrawingView_Clicked(object sender, EventArgs e)
        {
            DrawingCanvas?.Lines?.Clear();
        }

        private static async Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}
