using System.Windows;
using FacturacionMail.Views;

namespace FacturacionMail;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnConsultaFacturasClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new ConsultaFacturasView();
    }

    private void OnEnvioMailClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new FacturacionMailView();
    }

    private void OnFacturasPendientesClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new EnvioFacturasPendientesView();
    }

    private void OnEstadoEnviosClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new EstadoEnvioMailView();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}