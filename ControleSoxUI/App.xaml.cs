using ControleSoxUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using ControleSoxUI.ViewModels;

namespace ControleSoxUI
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<AppInitializer>();
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<Uam2ViewModel>();
                    services.AddTransient<Uam4ViewModel>();
                    services.AddTransient<Uam5ViewModel>();

                    // Ventanas
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            _host.Start();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null)
                await _host.StopAsync();
            base.OnExit(e);
        }
    }
}