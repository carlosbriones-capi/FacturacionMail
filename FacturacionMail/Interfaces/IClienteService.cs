using FacturacionMail.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionMail.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ObtenerClientesAsync();
    Task<IEnumerable<string>> ObtenerClientesExcluidosAsync();
}