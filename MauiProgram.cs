using CommunityToolkit.Maui;
using Jindal;
using Jindal.Services;
using Jindal.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Configure the app
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        // Add configuration
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Jindal.appsettings.json");
        if (stream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
        }

        // Register core services with proper lifetime management
        RegisterServices(builder.Services, builder.Configuration);
        
        // Configure logging with enhanced settings
        ConfigureLogging(builder.Logging);
        
        var app = builder.Build();
        
        // Initialize platform-specific components
        InitializePlatformSpecific();
        
        return app;
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Core Services (only register non-static services)
        services.AddSingleton<ValidationHelper>();
        services.AddSingleton<ErrorHandlingService>();
        
        // Initialize the existing DatabaseService with hybrid support
        // This is done at startup to provide configuration and logging
        // Note: Logger will be set up properly when the app starts
        DatabaseService.InitializeServices(configuration);
        
        // Network Services with basic configuration
        services.AddHttpClient<ApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "JindalGuestManagement/2.0.0");
        });

        services.AddSingleton<ConnectivityService>();
        
        // Register all Views for dependency injection
        RegisterViews(services);
    }

    private static void RegisterViews(IServiceCollection services)
    {
        // Register all views as transient
        services.AddTransient<MainPage>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<LocationPage>();
        services.AddTransient<RoomPage>();
        services.AddTransient<CheckInOutPage>();
        services.AddTransient<ReportPage>();
        services.AddTransient<UserManagementPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<LogoutPage>();
        services.AddTransient<Jindal.Views.LoadingPage>();
        services.AddTransient<ErrorPage>();
        
        // Modal/Detail pages
        services.AddTransient<AddCheckInOutPage>();
        services.AddTransient<AddEditLocationPage>();
        services.AddTransient<AddEditRoomPage>();
        services.AddTransient<AddEditUserPage>();
        services.AddTransient<AddGuestToSameRoomPage>();
        services.AddTransient<EditGuestPage>();
        services.AddTransient<CheckOutPage>();
    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.AddDebug();
        
#if DEBUG
        logging.SetMinimumLevel(LogLevel.Debug);
#else
        logging.SetMinimumLevel(LogLevel.Information);
#endif
        
        // Configure specific loggers
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("System", LogLevel.Warning);
        logging.AddFilter("Jindal", LogLevel.Debug);
    }

    private static void InitializePlatformSpecific()
    {
        // Initialize SQLite on Android
#if ANDROID
        SQLitePCL.Batteries_V2.Init();
        System.Diagnostics.Debug.WriteLine("SQLite initialized for Android");
#endif

        // Platform-specific initialization
#if WINDOWS
        System.Diagnostics.Debug.WriteLine("Windows platform initialized");
#endif

#if IOS
        System.Diagnostics.Debug.WriteLine("iOS platform initialized");
#endif

#if MACCATALYST
        System.Diagnostics.Debug.WriteLine("MacCatalyst platform initialized");
#endif
    }
}
