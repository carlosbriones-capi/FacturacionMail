using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FacturacionMail.Models;
using FacturacionMail.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace FacturacionMail.Services;
public class FacturaService : DatabaseServiceBase, IFacturaService
{
    private readonly IClienteService _clienteService;
    private readonly IAppLogger _logger;

    public FacturaService(IConfiguration config, IClienteService clienteService, IAppLogger logger) : base(config) 
    { 
        _clienteService = clienteService;
        _logger = logger;
    }

    public async Task<IEnumerable<Factura>> ObtenerFacturasAsync( string mesAnio, int clienteDesde, int clienteHasta, int facturaDesde, int facturaHasta, bool soloActuales)
    {
        var listaFacturas = new List<Factura>();
        string nombreProcedimiento = GetFunctionName("DameFacturas");
        
        int anyoMesNumerico = 0;
        if (DateTime.TryParseExact(mesAnio, "MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var fecha))
        {
            anyoMesNumerico = fecha.Year * 10000 + fecha.Month * 100 + 1;
        }

        /*var clientesExcluidos = await _clienteService.ObtenerClientesExcluidosAsync();
        string cadenaExcluidos = string.Join(",", clientesExcluidos);*/

        var cadenaExcluidos = "NULL";

        if (string.IsNullOrEmpty(cadenaExcluidos)) cadenaExcluidos = "NULL";

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            await using var transaccion = await conexion.BeginTransactionAsync();
            
            await using var command = new NpgsqlCommand(nombreProcedimiento, conexion);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = transaccion;

            command.Parameters.AddWithValue("anyomes", (decimal)anyoMesNumerico);
            command.Parameters.AddWithValue("cli_excluir", cadenaExcluidos);
            command.Parameters.AddWithValue("cli_desde", (decimal)clienteDesde);
            command.Parameters.AddWithValue("cli_hasta", (decimal)clienteHasta);
            command.Parameters.AddWithValue("fact_desde", (decimal)facturaDesde);
            command.Parameters.AddWithValue("fact_hasta", (decimal)facturaHasta);
            command.Parameters.AddWithValue("lista", 0m); 
            
            var nombreCursor = "ref_cursor_facturas";
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
                    var rutaArchivo = reader.GetValue(0)?.ToString()?.Trim().Replace("\r", "").Replace("\n", "") ?? string.Empty;
                    int.TryParse(reader.GetValue(1)?.ToString(), out int idLista);

                    listaFacturas.Add(new Factura
                    {
                        NombreArchivo = rutaArchivo,
                        NumeroFactura = Path.GetFileName(rutaArchivo),
                        ListaId = idLista, 
                        Seleccionada = true
                    });
                }
            }
            
            await transaccion.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener facturas desde la base de datos: {ex.Message}", ex);
        }

        return listaFacturas;
    }

    public async Task<IEnumerable<Factura>> ObtenerFacturasPendientesPorListaAsync(int listaId)
    {
        var listaFacturas = new List<Factura>();
        string nombreProcedimiento = GetFunctionName("DamePendientes");

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();
            await using var transaccion = await conexion.BeginTransactionAsync();
            
            await using var command = new NpgsqlCommand(nombreProcedimiento, conexion);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = transaccion;

            command.Parameters.AddWithValue("lista", (decimal)listaId);
            
            var nombreCursor = "ref_cursor_pendientes";
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
                    var rutaArchivo = reader.GetString(0)?.Trim().Replace("\r", "").Replace("\n", "") ?? string.Empty;
                    listaFacturas.Add(new Factura
                    {
                        NombreArchivo = rutaArchivo,
                        NumeroFactura = Path.GetFileName(rutaArchivo),
                        ListaId = listaId,
                        Seleccionada = true
                    });
                }
            }
            
            await transaccion.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener facturas pendientes: {ex.Message}", ex);
        }

        return listaFacturas;
    }

    public async Task VisualizarFacturaAsync(string rutaArchivo)
    {
        if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"No se encontró el archivo de la factura en: {rutaArchivo}");
        }

        string? rutaVisor = _config["ViewerSettings:PdfViewerPath"];

        try
        {
            if (string.IsNullOrEmpty(rutaVisor) || !File.Exists(rutaVisor))
            {
                // Abrir con el visor predeterminado si no hay uno específico configurado
                Process.Start(new ProcessStartInfo(rutaArchivo) { UseShellExecute = true });
            }
            else
            {
                // Abrir con el visor especificado (ej. Adobe Acrobat)
                Process.Start(rutaVisor, $"\"{rutaArchivo}\"");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al intentar visualizar el PDF: {ex.Message}", ex);
        }

        await Task.CompletedTask;
    }
}
