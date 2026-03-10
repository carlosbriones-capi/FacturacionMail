using FacturacionMail.Interfaces;
using FacturacionMail.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FacturacionMail.Services;

public class ClienteService : DatabaseServiceBase, IClienteService
{
    private readonly IAppLogger _logger;

    public ClienteService(IConfiguration config, IAppLogger logger) : base(config)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Cliente>> ObtenerClientesAsync()
    {
        var listaClientes = new List<Cliente>();
        string nombreFuncion = GetFunctionName("DameClientes");

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            await using var transaccion = await conexion.BeginTransactionAsync();

            await using var command = new NpgsqlCommand(nombreFuncion, conexion);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = transaccion;

            var parametroCursor = new NpgsqlParameter("recordset", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "ref_cursor_clientes"
            };
            command.Parameters.Add(parametroCursor);

            await command.ExecuteNonQueryAsync();

            await using var comandoFetch = new NpgsqlCommand("FETCH ALL FROM \"ref_cursor_clientes\"", conexion);
            comandoFetch.Transaction = transaccion;

            {
                await using var reader = await comandoFetch.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int codigoCliente = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));

                    listaClientes.Add(new Cliente
                    {
                        Codigo = codigoCliente,
                    });
                }
            }

            await transaccion.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener la lista de clientes desde la base de datos: {ex.Message}", ex);
        }

        return listaClientes;
    }

    public async Task<IEnumerable<string>> ObtenerClientesExcluidosAsync()
    {
        var codigosExcluidos = new List<string>();
        string nombreTabla = GetTableName("ClientesExcluidos");
        string query = $"Select GAB_CODCLI_EXCLUIDO from {nombreTabla} order by gab_codcli_excluido";

        try
        {
            await using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            await using var command = new NpgsqlCommand(query, conexion);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var valorCelda = reader.GetValue(0);
                if (valorCelda != null) codigosExcluidos.Add(valorCelda.ToString() ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener la lista de clientes excluidos: {ex.Message}", ex);
        }

        return codigosExcluidos;
    }
}
