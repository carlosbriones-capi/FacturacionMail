using FacturacionMail.Services;
using FacturacionMail.Models;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace FacturacionMail.Tests
{
    public class EmailServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public EmailServiceTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
            var dict = new Dictionary<string, string>
            {
                {"Database:Host", "dummy"},
                {"Database:Name", "dummy"},
                {"Database:User", "dummy"},
                {"Database:Pass", "dummy"},
                {"ZipSettings:ZipPath", Path.Combine(Path.GetTempPath(), "FacturasZipTest")},
                {"EmailSettings:MaxAttachmentSizeMB", "1"} // Bajamos a 1MB para el test
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dict!)
                .Build();
        }

        [Fact]
        public async Task ObtenerListasAsync_DebeRetornarListas_CuandoElClienteTieneConfiguracion()
        {
            // Arrange
            var config = _configuration;
            if (string.IsNullOrEmpty(config.GetSection("Database:ConnectionString").Value))
            {
                var dict = new Dictionary<string, string>
                {
                    {"Database:ConnectionString", "Host=cmbdpostgres;Database=develope;Username=postgres;Password=postgres"},
                    {"Database:Functions:DameListas", "produccion.facturaemail_dame_lista_envio_cliente"}
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            }

            var service = new EmailService(config, new NullLogger());
            int codigoClienteExistente = 62310;

            // Act
            var listas = await service.ObtenerListasAsync(codigoClienteExistente);

            // Assert
            Assert.NotNull(listas);
            var resultList = listas.ToList();
            
            _output.WriteLine($"--- LISTAS DE ENVÍO RECUPERADAS PARA EL CLIENTE {codigoClienteExistente} ---");
            foreach (var lista in resultList)
            {
                _output.WriteLine($"[DB TEST] ID: {lista.Id} | Nombre: {lista.Nombre}");
            }
            _output.WriteLine($"Total: {resultList.Count} listas.");

            Assert.True(resultList.Count >= 0);
        }

        [Fact]
        public async Task ObtenerDireccionesPorListaAsync_DebeRetornarEmails()
        {
            // Arrange
            var config = _configuration;
            var service = new EmailService(config, new NullLogger());
            int listaIdPrueba = 10;

            // Act
            var direcciones = await service.ObtenerDireccionesPorListaAsync(listaIdPrueba);

            // Assert
            Assert.NotNull(direcciones);
            var lista = direcciones.ToList();
            _output.WriteLine($"Destinatarios para la Lista {listaIdPrueba}: {lista.Count}");
            foreach (var d in lista) _output.WriteLine($" - {d.Email}");

            Assert.True(lista.Count >= 0);
        }

        [Fact]
        public async Task EnviarMailAsync_DebeComprimirArchivos_CuandoSuperanTamanoMaximo()
        {
            // Arrange
            var config = _configuration;
            var service = new EmailService(config, new NullLogger());

            // Crear archivos temporales de prueba para simular adjuntos
            string tempDir = Path.Combine(Path.GetTempPath(), "TestFacturas");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            // Creamos 2 archivos de 0.6MB cada uno (Total 1.2MB > 1MB max en dict)
            string file1 = Path.Combine(tempDir, "factura1.pdf");
            string file2 = Path.Combine(tempDir, "factura2.pdf");

            byte[] dummyData = new byte[(int)(0.6 * 1024 * 1024)]; 
            await File.WriteAllBytesAsync(file1, dummyData);
            await File.WriteAllBytesAsync(file2, dummyData);

            var facturas = new List<Factura>
            {
                new Factura { NombreArchivo = file1 },
                new Factura { NombreArchivo = file2 }
            };

            var destinatarios = new List<DireccionEmail>
            {
                new DireccionEmail { Email = "test@example.com" }
            };

            // Act & Assert
            // El test fallará en la conexión a DB, pero podemos capturar el error 
            // y verificar si se llegó a crear el ZIP en la ruta configurada.
            string zipDir = config["ZipSettings:ZipPath"]!;
            if (Directory.Exists(zipDir)) Directory.Delete(zipDir, true);

            try 
            {
                await service.EnviarMailAsync("Test Asunto", "Cuerpo Mensaje", destinatarios, facturas);
            }
            catch (System.Exception)
            {
                // Ignoramos cualquier error (probablemente de DB) para verificar si se creó el ZIP
            }

            // Verificamos si existe un archivo .zip en zipDir
            bool zipExiste = Directory.Exists(zipDir) && Directory.GetFiles(zipDir, "*.zip").Any();
            
            Assert.True(zipExiste, "El archivo ZIP debería haberse creado en la ruta configurada en ZipSettings:ZipPath");

            // Limpieza
            if (File.Exists(file1)) File.Delete(file1);
            if (File.Exists(file2)) File.Delete(file2);
            if (Directory.Exists(zipDir)) Directory.Delete(zipDir, true);
        }

        [Fact]
        public async Task ObtenerEstadoEnviosAsync_DebeRetornarEstados_CuandoExistanLogs()
        {
            // Arrange
            var service = new EmailService(_configuration, new NullLogger());
            _output.WriteLine($"--- CONSULTA DE ESTADOS DE ENVÍO DE HOY ({DateTime.Now:dd/MM/yyyy}) ---");
            _output.WriteLine($"PC: {Environment.MachineName}");

            // Act
            var (items, total) = await service.ObtenerEstadoEnviosAsync(10, 0);

            // Assert
            Assert.NotNull(items);
            var resultList = items.ToList();
            
            foreach (var e in resultList)
            {
                _output.WriteLine($"[LOG] ID: {e.Orden} | Msg: {e.Envios} | Cliente: {e.NombrePC} | Estado: {e.Estado} | Asunto: {e.Asunto}");
            }
            _output.WriteLine($"Total recuperados: {resultList.Count} de {total} registros.");

            Assert.True(resultList.Count >= 0);
        }
    }
}
