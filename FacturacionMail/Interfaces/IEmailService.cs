using FacturacionMail.Models;

namespace FacturacionMail.Interfaces;

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
    /// Obtiene las listas de envío (gablistamail) disponibles para un cliente.
    /// </summary>
    Task<IEnumerable<ListaEmail>> ObtenerListasAsync(int codigoCliente);

    /// <summary>
    /// Obtiene los destinatarios pertenecientes a una lista de envío concreta.
    /// </summary>
    Task<IEnumerable<DireccionEmail>> ObtenerDireccionesPorListaAsync(int listaId);

    /// <summary>
    /// Envía las facturas seleccionadas a las direcciones seleccionadas.
    /// </summary>
    Task<bool> EnviarMailAsync(
        string asunto,
        string cuerpo,
        IEnumerable<DireccionEmail> destinatarios,
        IEnumerable<Factura> facturas);

    /// <summary>
    /// Obtiene el estado de los envíos de mail.
    /// </summary>
    Task<IEnumerable<EstadoEnvioMail>> ObtenerEstadoEnviosAsync();
}
