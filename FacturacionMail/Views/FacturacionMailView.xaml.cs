using System.Windows.Controls;
using FacturacionMail.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FacturacionMail.Views;

public partial class FacturacionMailView : UserControl
{
    public FacturacionMailView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<FacturacionMailViewModel>();
    }
}
