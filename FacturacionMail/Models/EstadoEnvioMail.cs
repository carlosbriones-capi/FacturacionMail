using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

public partial class EstadoEnvioMail : ObservableObject
{
    public int Orden { get; set; }
    public DateTime Fecha { get; set; }
    public string HoraIni { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public int CodigoCliente { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public int Adjuntos { get; set; }
    public int Envios { get; set; }
    public string Estado { get; set; } = string.Empty;
}
