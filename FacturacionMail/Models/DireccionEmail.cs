using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FacturacionMail.Models;

/// <summary>
/// Dirección de email destino de envío de facturas.
/// Mapeado a gablistamail (lista de envío mail de un cliente).
/// </summary>
public class DireccionEmail : INotifyPropertyChanged
{
    private bool _seleccionada = true;

    public int    Id        { get; set; }
    public string Email     { get; set; } = string.Empty;
    public string Nombre    { get; set; } = string.Empty;
    public int    CodigoCliente { get; set; }

    public bool Seleccionada
    {
        get => _seleccionada;
        set { _seleccionada = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
