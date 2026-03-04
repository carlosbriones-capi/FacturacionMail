using FacturacionMail.Models;

namespace FacturacionMail.Services;

/// <summary>
/// Contrato para gestión de direcciones de email y envío de facturas.
/// Implementación real usará:
///   - gablistamail                   (listas de envío mail de un cliente)
///   - FACTURAEMAIL.DAMELISTACLI      (lista de envío de un cliente)
///   - FACTURAEMAIL.DAMEENVFACTURAS   (facturas a enviar)
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Obtiene la lista de direcciones de envío para un cliente.
    /// Equiv. a FACTURAEMAIL.DAMELISTACLI
    /// </summary>
    Task<IEnumerable<DireccionEmail>> ObtenerDireccionesAsync(int codigoCliente);

    /// <summary>
    /// Envía las facturas seleccionadas a las direcciones seleccionadas.
    /// </summary>
    Task<bool> EnviarMailAsync(
        string asunto,
        string cuerpo,
        IEnumerable<DireccionEmail> destinatarios,
        IEnumerable<Factura> facturas);
}
