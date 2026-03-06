using System.Windows.Controls;
using FacturacionMail.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FacturacionMail.Views;

public partial class EstadoEnvioMailView : UserControl
{
    public EstadoEnvioMailView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<EstadoEnvioMailViewModel>();
    }
}
