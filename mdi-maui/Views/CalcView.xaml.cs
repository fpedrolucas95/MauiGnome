using mdi_maui.ViewModels;

namespace mdi_maui.Views
{
    public partial class CalcView : ContentView
    {
        public CalcView()
        {
            InitializeComponent();
            BindingContext = new CalcViewModel();
        }
    }
}
