using FacturacionMail.Interfaces;
using System;

namespace FacturacionMail.Tests
{
    public class NullLogger : IAppLogger
    {
        public void ToLog(string mensaje) { }
        public void LogErr(string mensaje, Exception? ex = null) { }
        public void EnviaPresencia() { }
    }
}
