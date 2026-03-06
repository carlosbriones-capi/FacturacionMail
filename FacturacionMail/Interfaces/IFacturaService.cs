using FacturacionMail.Models;

namespace FacturacionMail.Interfaces;

/// <summary>
/// Contrato para obtención y filtrado de facturas.
/// Implementación real llamará a los stored procedures:
///   - FACTURAEMAIL.DAMEFACTURAS      (facturas actuales por mes-año)
///   - FACTURAEMAIL.DAMEFACTURASALL   (todas las facturas con filtros)
///   - FACTURAEMAIL.DAMEENVFACTURAS   (facturas a enviar)
/// </summary>
public interface IFacturaService
{
    /// <summary>
    /// Devuelve facturas según los criterios de selección.
    /// Equiv. a FACTURAEMAIL.DAMEFACTURAS / DAMEFACTURASALL
    /// </summary>
    Task<IEnumerable<Factura>> ObtenerFacturasAsync(
        string mesAnio,
        int clienteDesde,
        int clienteHasta,
        int facturaDesde,
        int facturaHasta,
        bool soloActuales);

    /// <summary>
    /// Devuelve la lista de clientes de facturación.
    /// Equiv. a FACTURAEMAIL.DAMECLIENVI
    /// </summary>
    Task<IEnumerable<Cliente>> ObtenerClientesAsync();
}
