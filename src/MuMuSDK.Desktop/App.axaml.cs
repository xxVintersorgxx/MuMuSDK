using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MuMuSDK.Core.Interfaces;
using MuMuSDK.Core.Services;
using MuMuSDK.Desktop.Models;
using MuMuSDK.Desktop.ViewModels;
using MuMuSDK.Desktop.Views;
using System;

namespace MuMuSDK.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            // 1. Core Static Extension Bridge Initialization Pass
            // Extracts your Singleton logging service and binds it straight to the static extension channel
            var activeBroker = Services.GetRequiredService<ILogBroker>();
            MuMuSDK.Core.Extensions.LogExtensions.Initialize(activeBroker);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = Services.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            // Tier A: Singletons (Global, persistent single-instance objects)
            // Load configuration settings cleanly from disk once, and share that memory globally
            services.AddSingleton<AppSettings>(provider => SettingsManager.Load());
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IProcessTracker, Win32ProcessTracker>();
            services.AddSingleton<ILogBroker, AsyncLogBroker>();
            services.AddSingleton<ITickerEngine, BackgroundTickerEngine>();
            services.AddSingleton<Win32MemoryMapper>();
            services.AddSingleton<IMemoryMapper>(provider => provider.GetRequiredService<Win32MemoryMapper>());
            services.AddSingleton<IMemoryReader>(provider => provider.GetRequiredService<Win32MemoryMapper>());
            // Tier B: Transients (Instantiated freshly every single time they are requested)
            services.AddTransient<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
        }
    }
}
