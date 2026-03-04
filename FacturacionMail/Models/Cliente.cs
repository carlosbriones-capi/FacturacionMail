namespace FacturacionMail.Models;

/// <summary>
/// Representa un cliente de facturación.
/// Mapeado a FACTURAEMAIL.DAMECLIENVI / FACTURAEMAIL.DAMELISTACLI
/// </summary>
public class Cliente
{
    public int    Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nif    { get; set; } = string.Empty;
}
