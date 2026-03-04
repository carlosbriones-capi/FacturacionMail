using System.Windows;
using System.Windows.Controls;
using FacturacionMail.ViewModels;

namespace FacturacionMail.Views;

public partial class ConsultaFacturasView : UserControl
{
    public ConsultaFacturasView()
    {
        InitializeComponent();
        DataContext = new ConsultaFacturasViewModel();
    }

    private void OnSalirClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow?.Close();
    }
}
