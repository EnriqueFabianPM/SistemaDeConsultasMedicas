using Microsoft.Extensions.Logging;
using CSM_BlazorHybridApp.ViewModels;

namespace CSM_BlazorHybridApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<Credentials>();
            builder.Services.AddSingleton<Authorization>();
            builder.Services.AddSingleton<NewUser>();
            builder.Services.AddSingleton<ApiConfig>();
            builder.Services.AddSingleton<UserResponse>();
            builder.Services.AddSingleton<ApiResponse>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
