using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

/// <summary>
/// Wrapper para Cliente con estado de selección para la UI.
/// </summary>
public partial class ClienteSeleccionable : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public Cliente Cliente { get; }

    public ClienteSeleccionable(Cliente cliente, bool isSelected = false)
    {
        Cliente = cliente;
        IsSelected = isSelected;
    }
}
