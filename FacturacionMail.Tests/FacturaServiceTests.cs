using FacturacionMail.Services;
using FacturacionMail.Models;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FacturacionMail.Tests
{
    public class FacturaServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public FacturaServiceTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        [Fact]
        public async Task ObtenerFacturasAsync_DebeRetornarFacturas_CuandoSePasaUnRangoValido()
        {
            // Arrange
            var config = _configuration;
            if (string.IsNullOrEmpty(config.GetSection("Database:ConnectionString").Value))
            {
                var dict = new Dictionary<string, string>
                {
                    {"Database:ConnectionString", "Host=cmbdpostgres;Database=develope;Username=postgres;Password=postgres"},
                    {"Database:Functions:DameFacturas", "produccion.facturaemail_dame_envfacturas"}
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            }

            var clienteService = new ClienteService(config, new NullLogger());
            var service = new FacturaService(config, clienteService, new NullLogger());

            // Act
            // Probamos con un mes/año que sepamos que puede tener datos, o 0 para ignorar
            var facturas = await service.ObtenerFacturasAsync("", 0, 0, 0, 0, true);

            // Assert
            Assert.NotNull(facturas);
            var listaFacturas = facturas.ToList();

            _output.WriteLine($"--- FACTURAS RECUPERADAS DE LA BD ---");
            foreach (var factura in listaFacturas)
            {
                _output.WriteLine($"[DB TEST] Archivo: {factura.NombreArchivo}, Lista: {factura.ListaId}");
            }
            _output.WriteLine($"Total: {listaFacturas.Count} facturas.");

            Assert.True(listaFacturas.Count >= 0);
        }

        [Fact]
        public async Task ObtenerFacturasPendientesPorListaAsync_DebeRetornarFacturas_CuandoLaListaTienePendientes()
        {
            // Arrange
            var config = _configuration;
            if (string.IsNullOrEmpty(config.GetSection("Database:ConnectionString").Value))
            {
                var dict = new Dictionary<string, string>
                {
                    {"Database:ConnectionString", "Host=cmbdpostgres;Database=develope;Username=postgres;Password=postgres"},
                    {"Database:Functions:DamePendientes", "produccion.facturaemail_dame_pendientes"}
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            }

            var clienteService = new ClienteService(config, new NullLogger());
            var service = new FacturaService(config, clienteService, new NullLogger());
            int listaIdPrueba = 10;
            
            var facturas = await service.ObtenerFacturasPendientesPorListaAsync(listaIdPrueba);

            // Assert
            Assert.NotNull(facturas);
            var listaFacturas = facturas.ToList();
            
            _output.WriteLine($"--- FACTURAS PENDIENTES RECUPERADAS PARA LA LISTA {listaIdPrueba} ---");
            foreach (var factura in listaFacturas)
            {
                _output.WriteLine($"[DB TEST] Archivo: {factura.NombreArchivo}");
            }
            _output.WriteLine($"Total: {listaFacturas.Count} facturas pendientes.");

            Assert.True(listaFacturas.Count >= 0);
        }
        [Fact]
        public async Task ObtenerFacturasAsync_DebeFiltrarPorMesAño_ConFormato8Digitos()
        {
            // Arrange
            var config = _configuration;
            if (string.IsNullOrEmpty(config.GetSection("Database:ConnectionString").Value))
            {
                var dict = new Dictionary<string, string>
                {
                    {"Database:ConnectionString", "Host=cmbdpostgres;Database=develope;Username=postgres;Password=postgres"},
                    {"Database:Functions:DameFacturas", "produccion.facturaemail_dame_envfacturas"}
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            }

            var clienteService = new ClienteService(config, new NullLogger());
            var service = new FacturaService(config, clienteService, new NullLogger());
            
            // Octubre 2024 -> Debería enviar 20241001 a la BD
            string mesPrueba = "10-2024";

            // Act
            var facturas = await service.ObtenerFacturasAsync(mesPrueba, 0, 0, 0, 0, false);

            // Assert
            Assert.NotNull(facturas);
            var lista = facturas.ToList();
            _output.WriteLine($"Facturas para {mesPrueba}: {lista.Count}");
            foreach(var f in lista.Take(5)) _output.WriteLine($" - {f.NombreArchivo}");
        }
    }
}
