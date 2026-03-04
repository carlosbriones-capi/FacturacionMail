using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

public partial class Factura : ObservableObject
{
    [ObservableProperty]
    private bool _seleccionada = true;

    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public int CodigoCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public string MesAnio { get; set; } = string.Empty;
}
