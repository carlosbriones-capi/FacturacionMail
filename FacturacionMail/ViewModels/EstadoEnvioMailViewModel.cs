using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Data;
using FacturacionMail.Models;
using FacturacionMail.Services;

namespace FacturacionMail.ViewModels;

public partial class EstadoEnvioMailViewModel : ObservableObject
{
    private readonly IEmailService _emailService;

    [ObservableProperty]
    private bool _ocupado;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    public ObservableCollection<EstadoEnvioMail> Estados { get; } = [];

    public EstadoEnvioMailViewModel()
    {
        _emailService = new MockDataService();
        _ = CargarDatosAsync();
    }

    [RelayCommand]
    private async Task CargarDatosAsync()
    {
        Ocupado = true;
        MensajeEstado = "Cargando estados de envío...";
        Estados.Clear();

        try
        {
            var data = await _emailService.ObtenerEstadoEnviosAsync();
            foreach (var item in data)
            {
                Estados.Add(item);
            }
            MensajeEstado = $"Se han cargado {Estados.Count} registros.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }
}
