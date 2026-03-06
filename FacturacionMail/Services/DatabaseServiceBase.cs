using System;
using Microsoft.Extensions.Configuration;

namespace FacturacionMail.Services
{
    /// <summary>
    /// Clase base abstracta para servicios que interactúan con la base de datos PostgreSQL.
    /// Centraliza la obtención de la cadena de conexión y nombres de tablas/funciones 
    /// desde la configuración de la aplicación.
    /// </summary>
    public abstract class DatabaseServiceBase
    {
        protected readonly string _connectionString;
        protected readonly IConfiguration _config;

        protected DatabaseServiceBase(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _connectionString = config.GetSection("Database:ConnectionString").Value 
                                ?? throw new InvalidOperationException("La cadena de conexión (ConnectionString) no está configurada en appsettings.json.");
        }

        /// <summary>
        /// Obtiene el nombre real de una tabla en la BD a partir de su clave en configuración.
        /// </summary>
        protected string GetTableName(string key) => 
            _config.GetSection($"Database:Tables:{key}").Value ?? key;

        /// <summary>
        /// Obtiene el nombre real de una función/stored procedure en la BD a partir de su clave en configuración.
        /// </summary>
        protected string GetFunctionName(string key) => 
            _config.GetSection($"Database:Functions:{key}").Value ?? key;
    }
}
