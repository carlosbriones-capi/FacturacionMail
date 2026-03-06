using System.Windows;
using System.Windows.Controls;
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
        UpdateSelection(MenuConsulta);
        MainContent.Content = new ConsultaFacturasView();
    }

    private void OnEnvioMailClick(object sender, RoutedEventArgs e)
    {
        UpdateSelection(MenuEnvio);
        MainContent.Content = new FacturacionMailView();
    }

    private void OnFacturasPendientesClick(object sender, RoutedEventArgs e)
    {
        UpdateSelection(MenuPendientes);
        MainContent.Content = new EnvioFacturasPendientesView();
    }

    private void OnEstadoEnviosClick(object sender, RoutedEventArgs e)
    {
        UpdateSelection(MenuEstado);
        MainContent.Content = new EstadoEnvioMailView();
    }

    private void UpdateSelection(MenuItem activeItem)
    {
        foreach (var item in new[] { MenuConsulta, MenuEnvio, MenuPendientes, MenuEstado })
        {
            if (item != null) item.IsChecked = (item == activeItem);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}