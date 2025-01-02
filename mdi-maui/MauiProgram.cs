using CommunityToolkit.Maui;
using mdi_maui.ViewModels;
using mdi_maui.Views;
using Microsoft.Extensions.Logging;

namespace mdi_maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(MauiAppBuilder builder)
    {
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CalcViewModel>();
        builder.Services.AddTransient<CalcView>();
        builder.Services.AddTransient<AboutViewModel>();
        builder.Services.AddTransient<AboutView>();
        builder.Services.AddTransient<PaintViewModel>();
        builder.Services.AddTransient<PaintView>();
        builder.Services.AddTransient<ChartViewModel>();
        builder.Services.AddTransient<ChartView>();
        builder.Services.AddTransient<ChatbotViewModel>();
        builder.Services.AddTransient<ChatbotView>();
    }
}
