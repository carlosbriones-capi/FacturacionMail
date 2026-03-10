using System;
using Microsoft.Extensions.Configuration;
using CM.Utils.Logs;
using FacturacionMail.Interfaces;

namespace FacturacionMail.Services;

public class CapiLoggerAdapter : IAppLogger, IDisposable
{
    private readonly CapiLogVerMonUDP _logger;

    public CapiLoggerAdapter(IConfiguration configuration)
    {
        var section = configuration.GetSection("Logging");
        
        string logPath = section["LogPath"] ?? "";
        string logName = section["LogName"] ?? "";
        string monitorIP = section["MonitorIP"] ?? "";
        int monitorPort = int.TryParse(section["MonitorPort"], out int p) ? p : 0;
        string appName = section["AppName"] ?? "";

        _logger = new CapiLogVerMonUDP(
            logPath, 
            logName, 
            true,
            true,
            0,
            0,
            null,
            monitorIP, 
            monitorPort, 
            appName,
            null!,
            null!
        );
    }

    public void ToLog(string mensaje)
    {
        _logger.ToLog(mensaje);
    }

    public void LogErr(string mensaje, Exception? ex = null)
    {
        if (ex != null)
            _logger.LogErr(ex, mensaje);
        else
            _logger.LogErr(mensaje);
    }

    public void EnviaPresencia()
    {
        _logger.EnviaPresencia();
    }

    public void Dispose()
    {
        _logger.Cierra();
    }
}
