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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}