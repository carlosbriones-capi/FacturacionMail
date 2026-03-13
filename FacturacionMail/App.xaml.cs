using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturacionMail.Interfaces;
using FacturacionMail.Services;
using FacturacionMail.ViewModels;
using System;
namespace FacturacionMail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration? Configuration { get; private set; }
        public static IServiceProvider? Services { get; private set; }

        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("FacturacionMail.ini", optional: false, reloadOnChange: false);
            
            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton(Configuration!);

            // Services
            services.AddSingleton<IAppLogger, CapiLoggerAdapter>();
            services.AddSingleton<IClienteService, ClienteService>();      
            services.AddSingleton<IFacturaService, FacturaService>();
            services.AddSingleton<IEmailService, EmailService>();

            // ViewModels
            services.AddTransient<FacturacionMailViewModel>();
            services.AddTransient<ConsultaFacturasViewModel>();
            services.AddTransient<EnvioFacturasPendientesViewModel>();
            services.AddTransient<EstadoEnvioMailViewModel>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logger = Services!.GetRequiredService<IAppLogger>();
            logger.ToLog("--- INICIO APLICACION FACTURACION MAIL ---");
            logger.EnviaPresencia();

            this.DispatcherUnhandledException += (s, args) =>
            {
                logger.LogErr("ERROR CRITICO NO CONTROLADO", args.Exception);
                MessageBox.Show($"Ocurrió un error inesperado: {args.Exception.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            var mainWindow = Services!.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var logger = Services?.GetService<IAppLogger>();
            logger?.ToLog("--- CIERRE APLICACION FACTURACION MAIL ---");
            
            if (logger is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }
}
