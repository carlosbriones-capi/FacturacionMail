using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FacturacionMail.Models;
using FacturacionMail.Interfaces;

namespace FacturacionMail.Services
{
    public class EmailService : DatabaseServiceBase, IEmailService
    {
        public EmailService(IConfiguration config) : base(config)
        {
        }

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
    }
}
