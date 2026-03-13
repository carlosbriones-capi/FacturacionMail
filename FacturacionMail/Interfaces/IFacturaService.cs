using FacturacionMail.Models;

namespace FacturacionMail.Interfaces;

public interface IFacturaService
{
    Task<IEnumerable<Factura>> ObtenerFacturasAsync( string mesAnio, int clienteDesde, int clienteHasta, int facturaDesde, int facturaHasta, bool soloActuales);

    Task<IEnumerable<Factura>> ObtenerFacturasPendientesPorListaAsync(int listaId);
    Task VisualizarFacturaAsync(string rutaPath);
}
