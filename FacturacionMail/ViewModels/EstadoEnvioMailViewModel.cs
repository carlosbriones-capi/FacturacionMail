using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Models;
using FacturacionMail.Services;
using FacturacionMail.Interfaces;

namespace FacturacionMail.ViewModels;

public partial class EstadoEnvioMailViewModel : ViewModelBase
{
    private readonly IEmailService _emailService;

    public ObservableCollection<EstadoEnvioMail> Estados { get; } = [];

    public EstadoEnvioMailViewModel(IEmailService emailService)
    {
        _emailService = emailService;
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
