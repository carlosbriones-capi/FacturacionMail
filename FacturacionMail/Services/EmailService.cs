using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FacturacionMail.Models;
using FacturacionMail.Interfaces;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace FacturacionMail.Services;
public class EmailService : DatabaseServiceBase, IEmailService
{
    private readonly IAppLogger _logger;

    public EmailService(IConfiguration config, IAppLogger logger) : base(config) 
    { 
        _logger = logger;
    }

    public async Task<IEnumerable<ListaEmail>> ObtenerListasAsync(int codigoCliente)
    {
        var listasEnvio = new List<ListaEmail>();
        string nombreProcedimiento = GetFunctionName("DameListas");

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            await using var transaccion = await conexion.BeginTransactionAsync();

            await using var command = new NpgsqlCommand(nombreProcedimiento, conexion);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = transaccion;

            command.Parameters.AddWithValue("cliente", (decimal)codigoCliente);
            
            var nombreCursor = "ref_cursor_listas";
            var parametroCursor = new NpgsqlParameter("recordset", NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = nombreCursor
            };
            command.Parameters.Add(parametroCursor);

            await command.ExecuteNonQueryAsync();

            await using var comandoFetch = new NpgsqlCommand($"FETCH ALL FROM \"{nombreCursor}\"", conexion);
            comandoFetch.Transaction = transaccion;
            
            await using (var reader = await comandoFetch.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    listasEnvio.Add(new ListaEmail
                    {
                        Id = int.TryParse(reader.GetValue(0).ToString(), out int id) ? id : 0,
                        Nombre = reader.GetString(1),
                        CodigoCliente = codigoCliente
                    });
                }
            }
            
            await transaccion.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener listas de envío del cliente: {ex.Message}", ex);
        }

        return listasEnvio;
    }

    public async Task<IEnumerable<DireccionEmail>> ObtenerDireccionesPorListaAsync(int listaId)
    {
        var listaDirecciones = new List<DireccionEmail>();
        string nombreTabla = GetTableName("ListasEnvio");
        
        string query = $"SELECT listaemail_email FROM {nombreTabla} WHERE listaemail_ref_listaenvio = @listaId AND (listaemail_envio = 1 OR listaemail_envio IS NULL) ORDER BY listaemail_email";

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            
            await using var command = new NpgsqlCommand(query, conexion);
            command.Parameters.AddWithValue("listaId", (decimal)listaId);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                listaDirecciones.Add(new DireccionEmail
                {
                    Email = reader.GetString(0),
                    Seleccionada = true
                });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener destinatarios de la lista de envío: {ex.Message}", ex);
        }

        return listaDirecciones;
    }

    public async Task<bool> EnviarMailAsync(string asunto, string cuerpo, IEnumerable<DireccionEmail> destinatarios, IEnumerable<Factura> facturas)
    {
        // 1. Obtener configuración
        var configuracionEmail = _config.GetSection("EmailSettings");
        var configuracionZip = _config.GetSection("ZipSettings");
        
        double tamanoMaximoPermitidoMB = configuracionEmail.GetValue<double>("MaxAttachmentSizeMB", 10);
        string rutaTemporalZip = configuracionZip["TempPath"] ?? Path.GetTempPath();
        
        if (!Directory.Exists(rutaTemporalZip)) 
        {
            Directory.CreateDirectory(rutaTemporalZip);
        }

        // 2. Calcular tamaño total de los archivos y verificar su existencia
        long totalBytesArchivos = 0;
        var listaRutasArchivos = new List<string>();
        foreach (var factura in facturas)
        {
            if (File.Exists(factura.NombreArchivo))
            {
                totalBytesArchivos += new FileInfo(factura.NombreArchivo).Length;
                listaRutasArchivos.Add(factura.NombreArchivo);
            }
        }

        double tamanoTotalMB = totalBytesArchivos / (1024.0 * 1024.0);
        bool requiereCompresion = tamanoTotalMB > tamanoMaximoPermitidoMB;
        string rutaAdjuntosFinales = string.Join(";", listaRutasArchivos);
        int contadorFicheros = listaRutasArchivos.Count;

        if (requiereCompresion && contadorFicheros > 0)
        {
            string nombreArchivoZip = $"Facturas_{DateTime.Now:yyyyMMddHHmmss}.zip";
            string rutaCompletaZip = Path.Combine(rutaTemporalZip, nombreArchivoZip);
            
            try
            {
                using (var flujoArchivo = new FileStream(rutaCompletaZip, FileMode.Create))
                using (var archivoZip = new ZipArchive(flujoArchivo, ZipArchiveMode.Create))
                {
                    foreach (var rutaOriginal in listaRutasArchivos)
                    {
                        archivoZip.CreateEntryFromFile(rutaOriginal, Path.GetFileName(rutaOriginal));
                    }
                }
                rutaAdjuntosFinales = rutaCompletaZip;
                contadorFicheros = 1; // El adjunto pasa a ser un único archivo .zip
                Console.WriteLine($"[MAIL] Archivos comprimidos por tamaño en: {rutaCompletaZip} (Tamaño: {tamanoTotalMB:F2}MB)");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error durante el proceso de compresión de archivos: {ex.Message}", ex);
            }
        }

        // 3. Registro en base de datos para procesamiento por el servicio de correo
        string nombreTabla = GetTableName("EnviosHistoricos");
        string sql = $@"INSERT INTO {nombreTabla} 
            (MAIL_ID, MAIL_FECHA, MAIL_HORAINI, MAIL_HORAFIN, MAIL_CLIENTE, MAIL_TIPO, MAIL_MSG, 
             MAIL_ASUNTO, MAIL_BODY, MAIL_ESTADO, MAIL_DIRECCIONES, MAIL_ADJUNTOS, MAIL_INTENTOS, 
             MAIL_TOTALFICHEROS, MAIL_OBSERVACIONES, MAIL_PRIORIDAD, MAIL_APLICACION, MAIL_OPERACION) 
            VALUES 
            (0, @fecha, @hora, 0, @cliente, 1, @msg, 
             @asunto, @body, 0, @direcciones, @adjuntos, 0, 
             0, '', 3, 'FACTURACION', '0')";

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            await using var command = new NpgsqlCommand(sql, conexion);
            
            command.Parameters.AddWithValue("fecha", decimal.Parse(DateTime.Now.ToString("yyyyMMdd")));
            command.Parameters.AddWithValue("hora", DateTime.Now.ToString("HHmmss"));
            command.Parameters.AddWithValue("cliente", Environment.MachineName);
            command.Parameters.AddWithValue("msg", (decimal)destinatarios.Count());
            command.Parameters.AddWithValue("asunto", asunto);
            command.Parameters.AddWithValue("body", cuerpo);
            command.Parameters.AddWithValue("direcciones", string.Join(";", destinatarios.Select(d => d.Email)));
            command.Parameters.AddWithValue("adjuntos", rutaAdjuntosFinales);

            await command.ExecuteNonQueryAsync();
            _logger.ToLog($"[MAIL] Registro de envío insertado en {nombreTabla} para {destinatarios.Count()} destinatario(s).");
        }
        catch (Exception ex)
        {
            //_logger.LogErr("Error al registrar el envío en la base de datos", ex);
            throw new Exception($"Error al registrar el envío: {ex.Message}", ex);
        }
        
        return true;
    }

    public async Task<IEnumerable<EstadoEnvioMail>> ObtenerEstadoEnviosAsync()
    {
        var listaEstados = new List<EstadoEnvioMail>();
        string nombreTabla = GetTableName("EnviosHistoricos");
        string query = $"select mail_id,mail_msg,mail_fecha, Mail_horaini, mail_horafin,mail_cliente,mail_tipo, mail_asunto,mail_totalficheros,mail_estado from {nombreTabla} where mail_fecha = @fecha and mail_aplicacion = 'FACTURACION' and mail_cliente = @PCName order by mail_msg, mail_id";

        try
        {
            decimal fechaHoy = decimal.Parse(DateTime.Now.ToString("yyyyMMdd"));
            string nombrePC = Environment.MachineName;

            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            await using var command = new NpgsqlCommand(query, conexion);
            command.Parameters.AddWithValue("fecha", fechaHoy);
            command.Parameters.AddWithValue("PCName", nombrePC);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var estado = new EstadoEnvioMail
                {
                    Orden = int.TryParse(reader.GetValue(0)?.ToString(), out int id) ? id : 0,
                    Envios = int.TryParse(reader.GetValue(1)?.ToString(), out int msg) ? msg : 0,
                    HoraIni = reader.GetValue(3)?.ToString() ?? string.Empty,
                    HoraFin = reader.GetValue(4)?.ToString() ?? string.Empty,
                    CodigoCliente = int.TryParse(reader.GetValue(5)?.ToString(), out int cli) ? cli : 0,
                    Tipo = reader.GetValue(6)?.ToString() ?? string.Empty,
                    Asunto = reader.GetValue(7)?.ToString() ?? string.Empty,
                    Adjuntos = int.TryParse(reader.GetValue(8)?.ToString(), out int adj) ? adj : 0,
                    Estado = reader.GetValue(9)?.ToString() ?? string.Empty
                };

                // Convertir decimal yyyyMMdd a DateTime
                if (decimal.TryParse(reader.GetValue(2)?.ToString(), out decimal f))
                {
                    string fs = f.ToString();
                    if (fs.Length == 8)
                    {
                        estado.Fecha = new DateTime(int.Parse(fs.Substring(0, 4)), int.Parse(fs.Substring(4, 2)), int.Parse(fs.Substring(6, 2)));
                    }
                }

                listaEstados.Add(estado);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener estados de envío: {ex.Message}", ex);
        }

        return listaEstados;
    }
}
