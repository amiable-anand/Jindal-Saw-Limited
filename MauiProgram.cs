using CommunityToolkit.Maui;
using Jindal;
using Jindal.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>() // This must come before CommunityToolkit
            .UseMauiCommunityToolkit() // Chain it here
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        // DatabaseService is static, no need to register
        
        var app = builder.Build();
        
        // Initialize SQLite on Android
        #if ANDROID
        SQLitePCL.Batteries_V2.Init();
        #endif
        
        return app;
    }
}
