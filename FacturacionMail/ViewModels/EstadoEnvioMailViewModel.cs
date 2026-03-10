using System;
using System.Collections.Generic;
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
    private int _paginaActual = 1;
    private int _registrosPorPagina = 17;
    private int _totalRegistros = 0;
    private int _totalPaginas = 1;

    public ObservableCollection<EstadoEnvioMail> Estados { get; } = [];
    public ObservableCollection<int> PaginasVisibles { get; } = [];

    public EstadoEnvioMailViewModel(IEmailService emailService)
    {
        _emailService = emailService;
        _ = CargarDatosAsync();
    }

    public int PaginaActual
    {
        get => _paginaActual;
        set 
        {
            if (SetProperty(ref _paginaActual, value))
            {
                OnPropertyChanged(nameof(CanIrAnterior));
                OnPropertyChanged(nameof(CanIrSiguiente));
                ActualizarPaginasVisibles();
                _ = CargarDatosAsync();
            }
        }
    }

    public int TotalPaginas
    {
        get => _totalPaginas;
        private set 
        {
            if (SetProperty(ref _totalPaginas, value))
            {
                ActualizarPaginasVisibles();
            }
        }
    }

    public bool CanIrAnterior => PaginaActual > 1;
    public bool CanIrSiguiente => PaginaActual < TotalPaginas;

    private void ActualizarPaginasVisibles()
    {
        PaginasVisibles.Clear();
        int max = 5;
        int start = Math.Max(1, PaginaActual - (max / 2));
        int end = Math.Min(TotalPaginas, start + max - 1);
        
        if (end - start + 1 < max && start > 1)
        {
            start = Math.Max(1, end - max + 1);
        }

        for (int i = start; i <= end; i++)
        {
            PaginasVisibles.Add(i);
        }
    }

    [RelayCommand]
    private void AnteriorPagina() { if (CanIrAnterior) PaginaActual--; }

    [RelayCommand]
    private void SiguientePagina() { if (CanIrSiguiente) PaginaActual++; }

    [RelayCommand]
    private void IrAPagina(int pagina)
    {
        if (pagina >= 1 && pagina <= TotalPaginas)
        {
            PaginaActual = pagina;
        }
    }

    [RelayCommand]
    private async Task CargarDatosAsync()
    {
        if (Ocupado) return;
        Ocupado = true;
        MensajeEstado = "Cargando registros...";
        
        try
        {
            int offset = (PaginaActual - 1) * _registrosPorPagina;
            var (items, total) = await _emailService.ObtenerEstadoEnviosAsync(_registrosPorPagina, offset);
            
            Estados.Clear();
            foreach (var item in items) Estados.Add(item);

            _totalRegistros = total;
            TotalPaginas = (int)Math.Ceiling((double)total / _registrosPorPagina);
            if (TotalPaginas == 0) TotalPaginas = 1;

            OnPropertyChanged(nameof(CanIrAnterior));
            OnPropertyChanged(nameof(CanIrSiguiente));
            
            MensajeEstado = $"Viendo registros {(total > 0 ? offset + 1 : 0)} a {Math.Min(offset + _registrosPorPagina, total)} de {total}";
        }
        catch (Exception ex)
        {
            ShowError($"Error al cargar datos: {ex.Message}");
        }
        finally
        {
            Ocupado = false;
        }
    }
}
