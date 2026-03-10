using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

public partial class Factura : ObservableObject
{
    [ObservableProperty]
    private bool _seleccionada = true;

    public string NombreArchivo { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public int ListaId { get; set; }
}
