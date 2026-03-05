using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

/// <summary>
/// Dirección de email destino de envío de facturas.
/// Mapeado a gablistamail (lista de envío mail de un cliente).
/// </summary>
public partial class DireccionEmail : ObservableObject
{
    [ObservableProperty]
    private bool _seleccionada = true;

    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CodigoCliente { get; set; }
    public int? ListaId { get; set; }

}
