namespace FacturacionMail.Models;

/// <summary>
/// Representa una lista de envío de correo para un cliente.
/// Mapeado a gablistamail (listas de envío mail de un cliente).
/// </summary>
public class ListaEmail
{
    public int    Id             { get; set; }
    public string Nombre         { get; set; } = string.Empty;
    public int    CodigoCliente  { get; set; }
}
