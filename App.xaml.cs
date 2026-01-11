using Microsoft.Extensions.DependencyInjection;
using SophicIoTManager.Services;
using SophicIoTManager.ViewModels;
using System;
using System.Windows;

namespace SophicIoTManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Configures Dependency Injection container and manages application lifecycle.
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private ServiceProvider? _serviceProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current application's service provider.
        /// </summary>
        public static IServiceProvider? Services { get; private set; }

        #endregion

        #region Application Lifecycle

        /// <summary>
        /// Configures services and starts the application.
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Configure the Dependency Injection container
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            Services = _serviceProvider;

            // Create and show the main window with injected ViewModel
            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };

            mainWindow.Show();
        }

        /// <summary>
        /// Cleans up resources when the application exits.
        /// </summary>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Dispose of the service provider to clean up resources
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        #endregion

        #region Service Configuration

        /// <summary>
        /// Registers all services and ViewModels with the DI container.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            // Register Services
            // Using Singleton for the device service to maintain state across the application
            services.AddSingleton<IDeviceService, MockDeviceService>();

            // Register ViewModels
            // MainViewModel is transient (new instance each time) - though typically only one is created
            services.AddTransient<MainViewModel>();

            // Future: Add additional services here
            // services.AddSingleton<INavigationService, NavigationService>();
            // services.AddSingleton<IDialogService, DialogService>();
            // services.AddSingleton<IMqttService, MqttService>(); // For real IoT
        }

        #endregion
    }
}
