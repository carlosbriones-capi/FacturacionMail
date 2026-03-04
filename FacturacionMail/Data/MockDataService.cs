using FacturacionMail.Models;
using FacturacionMail.Services;

namespace FacturacionMail.Data;

/// <summary>
/// Implementación de datos mock para desarrollo de UI.
/// En producción se reemplaza por implementaciones reales que llamen
/// a los stored procedures de FACTURAEMAIL.
/// </summary>
public class MockDataService : IFacturaService, IEmailService
{
    private static readonly List<Cliente> _clientes =
    [
        new() { Codigo = 182, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 200, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 215, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 230, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 245, Nombre = "Asset Management Pro",   Nif = "A28456789" },
    ];

    private static readonly List<DireccionEmail> _direcciones =
    [
        new() { Id = 1, CodigoCliente = 182, Email = "facturas@cmcapitalmarkets.es",    Nombre = "Dpto. Facturación" },
        new() { Id = 2, CodigoCliente = 182, Email = "contabilidad@cmcapitalmarkets.es", Nombre = "Contabilidad"      },
        new() { Id = 3, CodigoCliente = 182, Email = "direccion@cmcapitalmarkets.es",    Nombre = "Dirección General" },
        new() { Id = 4, CodigoCliente = 200, Email = "facturacion@inversionesglobales.es", Nombre = "Facturación"    },
        new() { Id = 5, CodigoCliente = 200, Email = "admin@inversionesglobales.es",    Nombre = "Administración"   },
    ];

    private static List<Factura> GenerarFacturas(string mesAnio, int clienteDesde, int clienteHasta,
                                                  int facturaDesde, int facturaHasta, bool soloActuales)
    {
        var facturas = new List<Factura>();
        var baseId = 149511;
        var clientes = _clientes.Where(c => c.Codigo >= clienteDesde && c.Codigo <= clienteHasta).ToList();
        if (clienteDesde == 0 && clienteHasta == 0) clientes = _clientes;

        var rand = new Random(42);
        int idx = facturaDesde > 0 ? facturaDesde : baseId;
        int hasta = facturaHasta > 0 ? facturaHasta : idx + 14;

        for (int i = idx; i <= hasta; i++)
        {
            var cliente = clientes[rand.Next(clientes.Count)];
            facturas.Add(new Factura
            {
                Id             = i,
                NumeroFactura  = $"F{i}",
                NombreArchivo  = $"F{i}.pdf",
                CodigoCliente  = cliente.Codigo,
                NombreCliente  = cliente.Nombre,
                Importe        = Math.Round((decimal)(rand.NextDouble() * 9000 + 1000), 2),
                MesAnio        = mesAnio,
                Seleccionada   = true
            });
        }
        return facturas;
    }

    // ── IFacturaService ──────────────────────────────────────────────

    public Task<IEnumerable<Factura>> ObtenerFacturasAsync(
        string mesAnio, int clienteDesde, int clienteHasta,
        int facturaDesde, int facturaHasta, bool soloActuales)
    {
        var facturas = GenerarFacturas(mesAnio, clienteDesde, clienteHasta,
                                       facturaDesde, facturaHasta, soloActuales);
        return Task.FromResult<IEnumerable<Factura>>(facturas);
    }

    public Task<IEnumerable<Cliente>> ObtenerClientesAsync()
        => Task.FromResult<IEnumerable<Cliente>>(_clientes);

    // ── IEmailService ────────────────────────────────────────────────

    public Task<IEnumerable<DireccionEmail>> ObtenerDireccionesAsync(int codigoCliente)
    {
        var dirs = codigoCliente == 0
            ? _direcciones
            : _direcciones.Where(d => d.CodigoCliente == codigoCliente).ToList();

        if (!dirs.Any()) dirs = _direcciones.Take(3).ToList();

        return Task.FromResult<IEnumerable<DireccionEmail>>(dirs);
    }

    public Task<bool> EnviarMailAsync(string asunto, string cuerpo,
        IEnumerable<DireccionEmail> destinatarios, IEnumerable<Factura> facturas)
    {
        // Simulación: siempre éxito
        return Task.FromResult(true);
    }
}
