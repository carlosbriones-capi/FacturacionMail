using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturacionMail.Interfaces;
using FacturacionMail.Services;
using FacturacionMail.Data;
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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

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
            // services.AddSingleton<IFacturaService, FacturaService>();
            // services.AddSingleton<IEmailService, EmailService>();
            
            // Using MockDataService while real BD is not fully connected
            services.AddSingleton<IFacturaService, MockDataService>();
            services.AddSingleton<IEmailService, MockDataService>();

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

            var mainWindow = Services!.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
