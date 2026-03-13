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
            _logger.ToLog($"[INFO] Obteniendo listas de envío para el cliente: {codigoCliente}");
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
            _logger.ToLog($"[ERROR] Error al obtener listas de envío del cliente {codigoCliente}: {ex.Message}");
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
            _logger.ToLog($"[INFO] Obteniendo destinatarios de la lista de envío ID: {listaId}");
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
            _logger.ToLog($"[ERROR] Error al obtener destinatarios de la lista de envío ID {listaId}: {ex.Message}");
            throw new Exception($"Error al obtener destinatarios de la lista de envío: {ex.Message}", ex);
        }

        return listaDirecciones;
    }

    public async Task<bool> EnviarMailAsync(string asunto, string cuerpo, IEnumerable<DireccionEmail> destinatarios, IEnumerable<Factura> facturas)
    {
        var configuracionEmail = _config.GetSection("EmailSettings");
        var configuracionZip = _config.GetSection("ZipSettings");
        
        double tamanoMaximoPermitidoMB = configuracionEmail.GetValue<double>("MaxAttachmentSizeMB", 10);
        string rutaTemporalZip = configuracionZip["ZipPath"] ?? Path.GetTempPath();
        
        if (!Directory.Exists(rutaTemporalZip)) 
        {
            Directory.CreateDirectory(rutaTemporalZip);
        }

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
        
        string rutaAdjuntosFinales;
        int contadorFicheros;

        if (requiereCompresion && listaRutasArchivos.Count > 0)
        {
            string nombreArchivoZip = $"Facturas_{DateTime.Now:yyyyMMddHHmmss}.zip";
            string rutaCompletaZip = Path.Combine(rutaTemporalZip, nombreArchivoZip);
            
            try
            {
                _logger.ToLog($"[INFO] Tamaño total ({tamanoTotalMB:F2} MB) supera el máximo ({tamanoMaximoPermitidoMB:F2} MB). Comprimiendo {listaRutasArchivos.Count} archivos en {rutaCompletaZip}");
                using (var flujoArchivo = new FileStream(rutaCompletaZip, FileMode.Create))
                using (var archivoZip = new ZipArchive(flujoArchivo, ZipArchiveMode.Create))
                {
                    foreach (var rutaOriginal in listaRutasArchivos)
                    {
                        archivoZip.CreateEntryFromFile(rutaOriginal, Path.GetFileName(rutaOriginal));
                    }
                }
                rutaAdjuntosFinales = rutaCompletaZip;
                contadorFicheros = 1;
                _logger.ToLog($"[INFO] Compresión finalizada correctamente. Ruta del adjunto: {rutaAdjuntosFinales}");
            }
            catch (Exception ex)
            {
                _logger.ToLog($"[ERROR] Error durante el proceso de compresión de archivos: {ex.Message}");
                throw new Exception($"Error durante el proceso de compresión de archivos: {ex.Message}", ex);
            }
        }
        else
        {
            rutaAdjuntosFinales = string.Join(";", listaRutasArchivos);
            contadorFicheros = listaRutasArchivos.Count;
            _logger.ToLog($"[INFO] No requiere compresión o no hay archivos. Ruta de adjuntos: {(contadorFicheros > 0 ? "lista de ficheros" : "vacia")} ({contadorFicheros} ficheros)");
        }

        string nombreTabla = GetTableName("EnviosHistoricos");
        string sql = $@"INSERT INTO {nombreTabla} 
            (mail_id, mail_fecha, mail_horaini, mail_horafin, mail_cliente, mail_tipo, mail_msg, 
             mail_asunto, mail_body, mail_estado, mail_direcciones, mail_adjuntos, mail_intentos, 
             mail_totalficheros, mail_observaciones, mail_prioridad, mail_aplicacion, mail_operacion,
             mail_coculta, mail_ccopia) 
            VALUES 
            (0, @fecha, @hora, 0, @cliente, 1, @msg, 
             @asunto, @body, 0, @direcciones, @adjuntos, 0, 
             @total, '', 3, 'FACTURACION', '0', '', '')";

        try
        {
            _logger.ToLog($"[INFO] Registrando orden de envío en base de datos con {contadorFicheros} adjunto(s).");
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            await using var command = new NpgsqlCommand(sql, conexion);
            
            command.Parameters.AddWithValue("fecha", decimal.Parse(DateTime.Now.ToString("yyyyMMdd")));
            command.Parameters.AddWithValue("hora", decimal.Parse(DateTime.Now.ToString("HHmmss")));
            command.Parameters.AddWithValue("cliente", Environment.MachineName);
            command.Parameters.AddWithValue("msg", (decimal)destinatarios.Count());
            command.Parameters.AddWithValue("asunto", asunto);
            command.Parameters.AddWithValue("body", cuerpo);
            command.Parameters.AddWithValue("direcciones", string.Join(";", destinatarios.Select(d => d.Email)));
            command.Parameters.AddWithValue("adjuntos", rutaAdjuntosFinales);
            command.Parameters.AddWithValue("total", (decimal)contadorFicheros);

            await command.ExecuteNonQueryAsync();
            _logger.ToLog($"[MAIL] Registro de envío insertado en {nombreTabla} para {destinatarios.Count()} destinatario(s).");
        }
        catch (Exception ex)
        {
            _logger.LogErr("Error al registrar el envio de correos:", ex);
            throw new Exception($"Error al registrar el envío: {ex.Message}", ex);
        }
        
        return true;
    }

    public async Task<(IEnumerable<EstadoEnvioMail> items, int total)> ObtenerEstadoEnviosAsync(int limit, int offset)
    {
        var listaEstados = new List<EstadoEnvioMail>();
        string nombreTabla = GetTableName("EnviosHistoricos");
        
        string countQuery = $"SELECT COUNT(*) FROM {nombreTabla} WHERE mail_aplicacion = 'FACTURACION'";
        string query = $"SELECT mail_id, mail_msg, mail_fecha, mail_horaini, mail_horafin, mail_cliente, mail_tipo, mail_asunto, mail_totalficheros, mail_estado " +
                       $"FROM {nombreTabla} WHERE mail_aplicacion = 'FACTURACION' " +
                       $"ORDER BY mail_id DESC LIMIT @limit OFFSET @offset";

        int totalDocs = 0;

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            await using (var cmdCount = new NpgsqlCommand(countQuery, conexion))
            {
                totalDocs = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
            }

            await using var command = new NpgsqlCommand(query, conexion);
            command.Parameters.AddWithValue("limit", limit);
            command.Parameters.AddWithValue("offset", offset);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var estado = new EstadoEnvioMail
                {
                    Orden = int.TryParse(reader.GetValue(0)?.ToString(), out int id) ? id : 0,
                    Envios = int.TryParse(reader.GetValue(1)?.ToString(), out int msg) ? msg : 0,
                    NombrePC = reader.GetValue(5)?.ToString() ?? string.Empty,
                    Tipo = reader.GetValue(6)?.ToString() ?? string.Empty,
                    Asunto = reader.GetValue(7)?.ToString() ?? string.Empty,
                    Adjuntos = int.TryParse(reader.GetValue(8)?.ToString(), out int adj) ? adj : 0
                };

                estado.HoraIni = FormatearHora(reader.GetValue(3)?.ToString());
                estado.HoraFin = FormatearHora(reader.GetValue(4)?.ToString());

                string valEstado = reader.GetValue(9)?.ToString() ?? "0";
                estado.Estado = valEstado switch
                {
                    "0" => "Pendiente",
                    "1" => "Enviado",
                    "2" => "Error",
                    _ => valEstado
                };

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
            _logger.ToLog($"[ERROR] Error al obtener los estados de envío de mail: {ex.Message}");
            throw new Exception($"Error al obtener estados de envío: {ex.Message}", ex);
        }

        return (listaEstados, totalDocs);
    }

    private string FormatearHora(string? hhmmss)
    {
        if (string.IsNullOrEmpty(hhmmss) || hhmmss == "0") return "";
        string h = hhmmss.PadLeft(6, '0');
        if (h.Length != 6) return hhmmss;
        return $"{h.Substring(0, 2)}:{h.Substring(2, 2)}:{h.Substring(4, 2)}";
    }
}
