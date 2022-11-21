using System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The custom entry point for the application
/// </summary>
public static class Entry
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Set up the services
        IServiceCollection services = new ServiceCollection();
        ConfigureServices(services, args);
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Create the Application, initialize it and start the message pump
        App app = new(serviceProvider);
        app.InitializeComponent();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection, string[] args)
    {
        // Add app related services
        serviceCollection.AddSingleton(new LaunchArguments(args));
        serviceCollection.AddSingleton<JumpListManager>();
        serviceCollection.AddSingleton<LoggerManager>();
        serviceCollection.AddSingleton<AppDataManager>();
        serviceCollection.AddTransient<StartupManager>();
        serviceCollection.AddTransient<LicenseManager>();
        serviceCollection.AddSingleton<GamesManager>();
        serviceCollection.AddSingleton<AppViewModel>();
        serviceCollection.AddSingleton<AppUserData>();
        serviceCollection.AddSingleton<IAppInstanceData, AppInstanceData>();
        serviceCollection.AddTransient<FileManager>();
        serviceCollection.AddTransient<IUpdaterManager, RCPUpdaterManager>();
        serviceCollection.AddTransient<GameBackups_Manager>();
        serviceCollection.AddSingleton<DeployableFilesManager>();
        serviceCollection.AddSingleton<IMessenger, WeakReferenceMessenger>();

        // Add the main window
        serviceCollection.AddSingleton<MainWindow>();
        serviceCollection.AddSingleton<MainWindowViewModel>();

        // Add the pages
        serviceCollection.AddSingleton<Page_Games_ViewModel>();
        serviceCollection.AddSingleton<Page_GamesNew_ViewModel>();
        serviceCollection.AddSingleton<Page_Progression_ViewModel>();
        serviceCollection.AddSingleton<Page_Utilities_ViewModel>();
        serviceCollection.AddSingleton<Page_Mods_ViewModel>();
        serviceCollection.AddSingleton<Page_Settings_ViewModel>();
        serviceCollection.AddSingleton<Page_About_ViewModel>();
        serviceCollection.AddSingleton<Page_Debug_ViewModel>();

        // Add UI managers
        serviceCollection.AddSingleton<IDialogBaseManager, RCPWindowDialogBaseManager>();
        serviceCollection.AddTransient<IMessageUIManager, RCPMessageUIManager>();
        serviceCollection.AddTransient<IBrowseUIManager, RCPBrowseUIManager>();
        serviceCollection.AddTransient<AppUIManager>();
    }
}