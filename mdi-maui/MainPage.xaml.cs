using mdi_maui.ViewModels;

namespace mdi_maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel(MDIContainer);
    }
}