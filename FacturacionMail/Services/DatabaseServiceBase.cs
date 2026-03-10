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
            
            var host = _config["Database:Host"];
            var db   = _config["Database:Name"];
            var user = _config["Database:User"];
            var pass = _config["Database:Pass"];
            var schema = _config["Database:Schema"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(db) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                throw new InvalidOperationException("Faltan parámetros de conexión a la base de datos en FacturacionMail.ini (Host, Name, User, Pass).");
            }

            var builder = new System.Text.StringBuilder();
            builder.Append($"Host={host};Database={db};Username={user};Password={pass}");
            
            if (!string.IsNullOrEmpty(schema))
            {
                builder.Append($";SearchPath={schema}");
            }

            _connectionString = builder.ToString();
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
