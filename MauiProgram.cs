using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;

namespace mg3.foundry;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Registrazione servizi
        builder.Services.AddSingleton<Features.FoundryCore.Services.FoundryServiceCli>();
        builder.Services.AddSingleton<Features.FoundryCore.Services.FoundryServiceApi>();
        
        // Registrazione converter
        builder.Services.AddSingleton<Converters.BoolToMessageStyleConverter>();
        builder.Services.AddSingleton<Converters.BoolToStatusTextConverter>();
        builder.Services.AddSingleton<Converters.BoolToAlignmentConverter>();
        
        // Registrazione servizi
        builder.Services.AddSingleton<Features.FoundryCore.Services.NotificationService>();
        builder.Services.AddSingleton<Features.FoundryCore.Services.ModelCacheService>();
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // Registrazione ViewModels
        builder.Services.AddTransient<Features.Home.MainViewModel>();
        builder.Services.AddTransient<Features.Chat.ChatViewModel>();
        builder.Services.AddTransient<Features.ModelManagement.ModelsViewModel>();
        builder.Services.AddTransient<Features.FoundryCore.ViewModels.NotificationCenterViewModel>();

        // Registrazione Pages
        builder.Services.AddTransient<Features.Home.MainPage>();
        builder.Services.AddTransient<Features.Chat.ChatPage>();
        builder.Services.AddTransient<Features.ModelManagement.ModelsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
