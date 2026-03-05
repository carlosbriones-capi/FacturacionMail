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
        new() { Codigo = 183, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 201, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 216, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 231, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 246, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 184, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 202, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 217, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 232, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 247, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 185, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 203, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 218, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 233, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 248, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 186, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 204, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 219, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 234, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 249, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 187, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 205, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 220, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 235, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 250, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 251, Nombre = "Tech Nova Solutions",      Nif = "A12345678" },
        new() { Codigo = 252, Nombre = "Consultoría Estratégica",   Nif = "B87654321" },
        new() { Codigo = 253, Nombre = "Logística del Sur SL",     Nif = "B41223344" },
        new() { Codigo = 254, Nombre = "Iberia Renovables SA",     Nif = "A99887766" },
        new() { Codigo = 255, Nombre = "Distribuciones Alimentarias", Nif = "A50607080" },
        new() { Codigo = 256, Nombre = "Servicios Financieros Levante", Nif = "B03445566" },
        new() { Codigo = 257, Nombre = "Construcciones Madrid",    Nif = "B28990011" },
        new() { Codigo = 258, Nombre = "Digital Marketing Agency",  Nif = "B08332211" },
        new() { Codigo = 259, Nombre = "Export Mediterráneo",      Nif = "A46112233" },
        new() { Codigo = 260, Nombre = "Sistemas de Seguridad",    Nif = "A28001122" },
        new() { Codigo = 261, Nombre = "Talleres Industriales",    Nif = "B50112233" },
        new() { Codigo = 262, Nombre = "Inmobiliaria Siglo XXI",   Nif = "A29334455" },
        new() { Codigo = 263, Nombre = "Asesoría Fiscal Unidos",   Nif = "B08445566" },
        new() { Codigo = 264, Nombre = "Transportes Rápidos SL",   Nif = "B41556677" },
        new() { Codigo = 265, Nombre = "Software & Co.",           Nif = "A28667788" },
        new() { Codigo = 182, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 200, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 215, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 230, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 245, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 183, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 201, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 216, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 231, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 246, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 184, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 202, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 217, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 232, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 247, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 185, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 203, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 218, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 233, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 248, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 186, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 204, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 219, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 234, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 249, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 187, Nombre = "CM Capital Markets",     Nif = "A28123456" },
        new() { Codigo = 205, Nombre = "Inversiones Globales SA", Nif = "B28654321" },
        new() { Codigo = 220, Nombre = "Fondos Mediterráneo SL", Nif = "B08123789" },
        new() { Codigo = 235, Nombre = "Capital Invest Group",   Nif = "A81987654" },
        new() { Codigo = 250, Nombre = "Asset Management Pro",   Nif = "A28456789" },
        new() { Codigo = 251, Nombre = "Tech Nova Solutions",      Nif = "A12345678" },
        new() { Codigo = 252, Nombre = "Consultoría Estratégica",   Nif = "B87654321" },
        new() { Codigo = 253, Nombre = "Logística del Sur SL",     Nif = "B41223344" },
        new() { Codigo = 254, Nombre = "Iberia Renovables SA",     Nif = "A99887766" },
        new() { Codigo = 255, Nombre = "Distribuciones Alimentarias", Nif = "A50607080" },
        new() { Codigo = 256, Nombre = "Servicios Financieros Levante", Nif = "B03445566" },
        new() { Codigo = 257, Nombre = "Construcciones Madrid",    Nif = "B28990011" },
        new() { Codigo = 258, Nombre = "Digital Marketing Agency",  Nif = "B08332211" },
        new() { Codigo = 259, Nombre = "Export Mediterráneo",      Nif = "A46112233" },
        new() { Codigo = 260, Nombre = "Sistemas de Seguridad",    Nif = "A28001122" },
        new() { Codigo = 261, Nombre = "Talleres Industriales",    Nif = "B50112233" },
        new() { Codigo = 262, Nombre = "Inmobiliaria Siglo XXI",   Nif = "A29334455" },
        new() { Codigo = 263, Nombre = "Asesoría Fiscal Unidos",   Nif = "B08445566" },
        new() { Codigo = 264, Nombre = "Transportes Rápidos SL",   Nif = "B41556677" },
        new() { Codigo = 265, Nombre = "Software & Co.",           Nif = "A28667788" },
    ];

    private static readonly List<ListaEmail> _listas =
    [
        // CM Capital Markets (182)
        new() { Id = 1, CodigoCliente = 182, Nombre = "Lista Principal" },
        new() { Id = 2, CodigoCliente = 182, Nombre = "Dirección y Contabilidad" },
        // Inversiones Globales (200)
        new() { Id = 3, CodigoCliente = 200, Nombre = "Facturación" },
        // Tech Nova Solutions (251)
        new() { Id = 4, CodigoCliente = 251, Nombre = "Contactos Generales" },
        // Iberia Renovables (254)
        new() { Id = 5, CodigoCliente = 254, Nombre = "Gerencia y Pagos" },
        // Software & Co. (265)
        new() { Id = 6, CodigoCliente = 265, Nombre = "Equipo Técnico" },
        new() { Id = 7, CodigoCliente = 265, Nombre = "Contabilidad" },
    ];

    private static readonly List<DireccionEmail> _direcciones =
    [
        // Lista 1 — CM Capital Markets: Lista Principal
        new() { Id = 1, CodigoCliente = 182, Email = "facturas@cmcapitalmarkets.es",     Nombre = "Dpto. Facturación",  ListaId = 1 },
        new() { Id = 2, CodigoCliente = 182, Email = "contabilidad@cmcapitalmarkets.es", Nombre = "Contabilidad",       ListaId = 1 },
        // Lista 2 — CM Capital Markets: Dirección y Contabilidad
        new() { Id = 3, CodigoCliente = 182, Email = "direccion@cmcapitalmarkets.es",    Nombre = "Dirección General",  ListaId = 2 },
        new() { Id = 4, CodigoCliente = 182, Email = "contabilidad@cmcapitalmarkets.es", Nombre = "Contabilidad",       ListaId = 2 },
        // Lista 3 — Inversiones Globales: Facturación
        new() { Id = 5, CodigoCliente = 200, Email = "facturacion@inversionesglobales.es", Nombre = "Facturación",     ListaId = 3 },
        new() { Id = 6, CodigoCliente = 200, Email = "admin@inversionesglobales.es",     Nombre = "Administración",    ListaId = 3 },
        // Lista 4 — Tech Nova Solutions: Contactos Generales
        new() { Id = 7,  CodigoCliente = 251, Email = "soporte@technova.com",  Nombre = "Soporte Técnico",    ListaId = 4 },
        new() { Id = 8,  CodigoCliente = 251, Email = "admin@technova.com",    Nombre = "Administración",     ListaId = 4 },
        // Lista 5 — Iberia Renovables: Gerencia y Pagos
        new() { Id = 9,  CodigoCliente = 254, Email = "pagos@iberiarenovables.es",    Nombre = "Cuentas a Pagar", ListaId = 5 },
        new() { Id = 10, CodigoCliente = 254, Email = "gerencia@iberiarenovables.es", Nombre = "Gerencia",        ListaId = 5 },
        // Lista 6 — Software & Co.: Equipo Técnico
        new() { Id = 11, CodigoCliente = 265, Email = "dev@softwareandco.com",    Nombre = "Desarrollo",      ListaId = 6 },
        new() { Id = 12, CodigoCliente = 265, Email = "qa@softwareandco.com",     Nombre = "QA / Testing",    ListaId = 6 },
        // Lista 7 — Software & Co.: Contabilidad
        new() { Id = 13, CodigoCliente = 265, Email = "contas@softwareandco.com", Nombre = "Contabilidad",    ListaId = 7 },
        // Otros (sin lista — usados por ConsultaFacturasView)
        new() { Id = 14, CodigoCliente = 252, Email = "info@estrategica.es",         Nombre = "Información General" },
        new() { Id = 15, CodigoCliente = 253, Email = "logistica@delsur.es",         Nombre = "Dpto. Logística" },
        new() { Id = 16, CodigoCliente = 255, Email = "pedidos@distalimentarias.com", Nombre = "Pedidos" },
        new() { Id = 17, CodigoCliente = 258, Email = "billing@digitalagency.io",    Nombre = "Billing Dept" },
        new() { Id = 18, CodigoCliente = 260, Email = "central@seguridadglobal.com", Nombre = "Centralita" },
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

    public Task<IEnumerable<ListaEmail>> ObtenerListasAsync(int codigoCliente)
    {
        var listas = codigoCliente == 0
            ? _listas
            : _listas.Where(l => l.CodigoCliente == codigoCliente).ToList();
        return Task.FromResult<IEnumerable<ListaEmail>>(listas);
    }

    public Task<IEnumerable<DireccionEmail>> ObtenerDireccionesPorListaAsync(int listaId)
    {
        var dirs = _direcciones.Where(d => d.ListaId == listaId).ToList();
        return Task.FromResult<IEnumerable<DireccionEmail>>(dirs);
    }

    public Task<bool> EnviarMailAsync(string asunto, string cuerpo,
        IEnumerable<DireccionEmail> destinatarios, IEnumerable<Factura> facturas)
    {
        // Simulación: siempre éxito
        return Task.FromResult(true);
    }

    public Task<IEnumerable<EstadoEnvioMail>> ObtenerEstadoEnviosAsync()
    {
        var estados = new List<EstadoEnvioMail>
        {
            new() { Orden = 1, Fecha = DateTime.Now, HoraIni = "09:00", HoraFin = "09:05", Cliente = "CM Capital Markets", Tipo = "Factura", Asunto = "Facturas Feb-2026", Adjuntos = 5, Envios = 2, Estado = "Enviado" },
            new() { Orden = 2, Fecha = DateTime.Now, HoraIni = "09:10", HoraFin = "09:12", Cliente = "Inversiones Globales SA", Tipo = "Factura", Asunto = "Facturas Feb-2026", Adjuntos = 3, Envios = 1, Estado = "Error" },
            new() { Orden = 3, Fecha = DateTime.Now, HoraIni = "09:15", HoraFin = "09:20", Cliente = "Fondos Mediterráneo SL", Tipo = "Factura", Asunto = "Factura 12345", Adjuntos = 1, Envios = 1, Estado = "Enviado" },
            new() { Orden = 4, Fecha = DateTime.Now.AddDays(-1), HoraIni = "16:00", HoraFin = "16:10", Cliente = "Capital Invest Group", Tipo = "Factura", Asunto = "Resumen Mensual", Adjuntos = 12, Envios = 4, Estado = "Enviado" },
            new() { Orden = 5, Fecha = DateTime.Now.AddDays(-2), HoraIni = "10:30", HoraFin = "10:35", Cliente = "Asset Management Pro", Tipo = "Listado", Asunto = "Listado de Operaciones", Adjuntos = 2, Envios = 1, Estado = "Enviado" }
        };
        return Task.FromResult<IEnumerable<EstadoEnvioMail>>(estados);
    }
}
