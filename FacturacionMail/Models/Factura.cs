using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FacturacionMail.Models;

public class Factura : INotifyPropertyChanged
{
    private bool _seleccionada = true;

    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public int CodigoCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public string MesAnio { get; set; } = string.Empty;

    public bool Seleccionada
    {
        get => _seleccionada;
        set { _seleccionada = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
