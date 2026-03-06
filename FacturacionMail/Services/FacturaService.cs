using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FacturacionMail.Models;
using FacturacionMail.Interfaces;

namespace FacturacionMail.Services
{
    public class FacturaService : DatabaseServiceBase, IFacturaService
    {
        public FacturaService(IConfiguration config) : base(config)
        {
        }

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
    }
}
