using FacturacionMail.Interfaces;
using System;

namespace FacturacionMail.Tests
{
    public class NullLogger : IAppLogger
    {
        public void ToLog(string mensaje) { Console.WriteLine(mensaje); }
        public void LogErr(string mensaje, Exception? ex = null) { Console.WriteLine($"[ERROR] {mensaje} {ex?.Message}"); }
        public void EnviaPresencia() { }
    }
}
