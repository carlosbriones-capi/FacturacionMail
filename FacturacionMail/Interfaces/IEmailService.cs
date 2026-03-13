using System.Threading.Tasks;
using System.Collections.Generic;
using FacturacionMail.Models;

namespace FacturacionMail.Interfaces;

public interface IEmailService
{
    Task<IEnumerable<ListaEmail>> ObtenerListasAsync(int codigoCliente);
    Task<IEnumerable<DireccionEmail>> ObtenerDireccionesPorListaAsync(int listaId);
    Task<bool> EnviarMailAsync(string asunto, string cuerpo, IEnumerable<DireccionEmail> destinatarios, IEnumerable<Factura> facturas);
    Task<(IEnumerable<EstadoEnvioMail> items, int total)> ObtenerEstadoEnviosAsync(int limit, int offset);
}
