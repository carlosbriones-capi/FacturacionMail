using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using FacturacionMail.Models;

namespace FacturacionMail.Services
{
    /// <summary>
    /// Implementación real de los servicios de datos conectada a PostgreSQL.
    /// Lee la configuración desde appsettings.json.
    /// </summary>
    public class DatabaseService : IFacturaService, IEmailService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public DatabaseService(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetSection("Database:ConnectionString").Value 
                                ?? throw new InvalidOperationException("ConnectionString no configurada.");
        }

        private string GetTableName(string key) => _config.GetSection($"Database:Tables:{key}").Value ?? key;
        private string GetFunctionName(string key) => _config.GetSection($"Database:Functions:{key}").Value ?? key;

        #region IFacturaService Implementation

        public async Task<IEnumerable<Factura>> ObtenerFacturasAsync(
            string mesAnio,
            int clienteDesde,
            int clienteHasta,
            int facturaDesde,
            int facturaHasta,
            bool soloActuales)
        {
            var facturas = new List<Factura>();
            string funcName = GetFunctionName(soloActuales ? "DameFacturas" : "DameFacturasAll");
            
            // using (var conn = new NpgsqlConnection(_connectionString))
            // {
            //     await conn.OpenAsync();
            //     using (var cmd = new NpgsqlCommand(funcName, conn))
            //     {
            //         cmd.CommandType = CommandType.StoredProcedure;
            //         cmd.Parameters.AddWithValue("p_mes_anio", mesAnio);
            //         // ...
            //     }
            // }

            return facturas;
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesAsync()
        {
            var clientes = new List<Cliente>();
            string funcName = GetFunctionName("DameClientes");

            // using (var conn = new NpgsqlConnection(_connectionString))
            // {
            //     await conn.OpenAsync();
            //     using (var cmd = new NpgsqlCommand(funcName, conn))
            //     {
            //         cmd.CommandType = CommandType.StoredProcedure;
            //         // ... ejecutar y mapear
            //     }
            // }

            return clientes;
        }

        #endregion

        #region IEmailService Implementation

        public async Task<IEnumerable<DireccionEmail>> ObtenerDireccionesAsync(int codigoCliente)
        {
            var direcciones = new List<DireccionEmail>();

            // TODO: Implementar llamada a FACTURAEMAIL.DAMELISTACLI o similar
            
            return direcciones;
        }

        public async Task<IEnumerable<ListaEmail>> ObtenerListasAsync(int codigoCliente)
        {
            var listas = new List<ListaEmail>();

            // TODO: Consultar gablistamail por cliente
            
            return listas;
        }

        public async Task<IEnumerable<DireccionEmail>> ObtenerDireccionesPorListaAsync(int listaId)
        {
            var direcciones = new List<DireccionEmail>();

            // TODO: Consultar destinatarios de la lista concreta
            
            return direcciones;
        }

        public async Task<bool> EnviarMailAsync(
            string asunto,
            string cuerpo,
            IEnumerable<DireccionEmail> destinatarios,
            IEnumerable<Factura> facturas)
        {
            // 1. Obtener configuración
            var emailSettings = _config.GetSection("EmailSettings");
            var zipSettings = _config.GetSection("ZipSettings");
            
            double maxSizeMB = emailSettings.GetValue<double>("MaxAttachmentSizeMB", 10);
            string zipExe = zipSettings["ZipExecutablePath"] ?? "";
            string tempPath = zipSettings["TempPath"] ?? Path.GetTempPath();

            // 2. Calcular tamaño total de los archivos (ficticio por ahora)
            // Supongamos que facturas.Sum(f => GetFileSize(f.Path)) > maxSizeMB
            bool requiereCompresion = false; // Lógica: totalBytes / (1024 * 1024) > maxSizeMB

            if (requiereCompresion)
            {
                // TODO: Ejecutar proceso externo de compresión
                // ProcessStartInfo startInfo = new ProcessStartInfo
                // {
                //     FileName = zipExe,
                //     Arguments = $"a -tzip \"{tempPath}\\adjuntos.zip\" \"{archivosSource}\"",
                //     CreateNoWindow = true,
                //     UseShellExecute = false
                // };
                // using var process = Process.Start(startInfo);
                // await process!.WaitForExitAsync();
            }

            // 3. Enviar vía SMTP (Realizar envío...)
            await Task.Delay(500); // Simulación
            return true;
        }

        public async Task<IEnumerable<EstadoEnvioMail>> ObtenerEstadoEnviosAsync()
        {
            var estados = new List<EstadoEnvioMail>();
            
            // TODO: Consultar tabla de logs de envíos
            
            return estados;
        }

        #endregion
    }
}
