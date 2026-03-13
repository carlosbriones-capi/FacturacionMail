using FacturacionMail.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FacturacionMail.Views;

public partial class EnvioFacturasPendientesView : UserControl
{
    public EnvioFacturasPendientesView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<EnvioFacturasPendientesViewModel>();
    }
}
