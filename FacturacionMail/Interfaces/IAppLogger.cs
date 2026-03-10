using System;

namespace FacturacionMail.Interfaces;

public interface IAppLogger
{
    void ToLog(string mensaje);
    void LogErr(string mensaje, Exception? ex = null);
    void EnviaPresencia();
}
