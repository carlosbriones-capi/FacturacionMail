using System.Windows;
using System.Windows.Controls;
using FacturacionMail.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FacturacionMail.Views;

public partial class ConsultaFacturasView : UserControl
{
    public ConsultaFacturasView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<ConsultaFacturasViewModel>();
    }

    private void OnSalirClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow?.Close();
    }
}
