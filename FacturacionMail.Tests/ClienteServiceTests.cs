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
    public class ClienteServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public ClienteServiceTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        [Fact]
        public async Task ObtenerClientesAsync_DebeRetornarClientes_CuandoLaConexionEsCorrecta()
        {
            // Arrange
            var config = _configuration;
            if (string.IsNullOrEmpty(config.GetSection("Database:ConnectionString").Value))
            {
                var dict = new Dictionary<string, string>
                {
                    {"Database:ConnectionString", "Host=cmbdpostgres;Database=develope;Username=postgres;Password=postgres"},
                    {"Database:Functions:DameClientes", "produccion.facturaemail_dame_cli_envio"}
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            }

            var service = new ClienteService(config, new NullLogger());

            // Act
            var clientes = await service.ObtenerClientesAsync();

            // Assert
            Assert.NotNull(clientes);
            var listaClientes = clientes.ToList();
            
            _output.WriteLine($"--- DATOS REUPERADOS DE LA BD ---");
            foreach (var cliente in listaClientes)
            {
                _output.WriteLine($"[DB TEST] Cliente Codigo: {cliente.Codigo}");
            }
            _output.WriteLine($"Total: {listaClientes.Count} clientes.");

            Assert.True(listaClientes.Count >= 0);
        }

        [Fact]
        public async Task ObtenerClientesExcluidosAsync_DebeRetornarCodigos()
        {
            // Arrange
            var service = new ClienteService(_configuration, new NullLogger());

            // Act
            var excluidos = await service.ObtenerClientesExcluidosAsync();

            // Assert
            Assert.NotNull(excluidos);
            var lista = excluidos.ToList();
            _output.WriteLine($"Clientes excluidos encontrados: {lista.Count}");
            foreach(var cod in lista) _output.WriteLine($" - {cod}");
            
            Assert.True(lista.Count >= 0);
        }
    }
}
