using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Data;
using FacturacionMail.Models;
using FacturacionMail.Services;
using FacturacionMail.Interfaces;

namespace FacturacionMail.ViewModels;

public partial class ConsultaFacturasViewModel : ViewModelBase
{
    private readonly IFacturaService _facturaService;
    private readonly IEmailService _emailService;

    // ── Criterios de selección ───────────────────────────────────────

    [ObservableProperty]
    private int _clienteDesde;

    [ObservableProperty]
    private int _clienteHasta;

    [ObservableProperty]
    private int _facturaDesde;

    [ObservableProperty]
    private int _facturaHasta;

    [ObservableProperty]
    private string _mesAnio = DateTime.Now.ToString("MM-yyyy");

    [ObservableProperty]
    private bool _soloActuales = true;

    // ── Email ────────────────────────────────────────────────────────

    public EmailFormViewModel EmailForm { get; }

    // ── Estado UI ────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnviarMailCommand))]
    private bool _resultadosVisibles = false;

    // ── Colecciones ───────────────────────────────────────────────────

    public ObservableCollection<Factura> Facturas { get; } = [];
    public ObservableCollection<DireccionEmail> Direcciones { get; } = [];

    // ── Selección Global ──────────────────────────────────────────────

    [ObservableProperty]
    private bool _seleccionarTodasFacturas = false;

    partial void OnSeleccionarTodasFacturasChanged(bool value)
    {
        if (_isUpdatingSelection) return;
        _isUpdatingSelection = true;
        foreach (var f in Facturas) f.Seleccionada = value;
        _isUpdatingSelection = false;
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private bool _seleccionarTodasDirecciones = false;

    partial void OnSeleccionarTodasDireccionesChanged(bool value)
    {
        if (_isUpdatingSelection) return;
        _isUpdatingSelection = true;
        foreach (var d in Direcciones) d.Seleccionada = value;
        _isUpdatingSelection = false;
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    private bool _isUpdatingSelection = false;

    private void CheckGlobalSelectionStates()
    {
        if (_isUpdatingSelection) return;
        _isUpdatingSelection = true;

        SeleccionarTodasFacturas = Facturas.Any() && Facturas.All(f => f.Seleccionada);
        SeleccionarTodasDirecciones = Direcciones.Any() && Direcciones.All(d => d.Seleccionada);

        _isUpdatingSelection = false;
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    public ConsultaFacturasViewModel(IFacturaService facturaService, IEmailService emailService)
    {
        _facturaService = facturaService;
        _emailService = emailService;

        EmailForm = new EmailFormViewModel(
            "Facturas C.M. Capital Markets",
            "CM Capital Markets\nOchandiano, 2\n28023 Madrid\nTelf: +34 91 509 62 61\nE-mail: facturas@capi.es",
            OnInsertarDireccion);
        
        // Listen to collection changes to update EnviarMailCommand state
        Direcciones.CollectionChanged += (s, e) => {
            CheckGlobalSelectionStates();
        };
    }

    private void OnInsertarDireccion(string direccion)
    {
        Direcciones.Add(new DireccionEmail
        {
            Id = Direcciones.Count + 100,
            Email = direccion,
            Nombre = "Manual",
            Seleccionada = true
        });
    }

    protected override void OnOcupadoChangedVirtual(bool value)
    {
        ProcesarSeleccionCommand.NotifyCanExecuteChanged();
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    // ── Comandos ──────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanProcesar))]
    private async Task ProcesarSeleccionAsync()
    {
        Ocupado = true;
        MensajeEstado = "Procesando selección...";
        ResultadosVisibles = false;

        try
        {
            var facturas = await _facturaService.ObtenerFacturasAsync(
                MesAnio, ClienteDesde, ClienteHasta, FacturaDesde, FacturaHasta, SoloActuales);

            var dirs = await _emailService.ObtenerDireccionesAsync(
                ClienteDesde == ClienteHasta && ClienteDesde > 0 ? ClienteDesde : 0);

            Facturas.Clear();
            foreach (var f in facturas) 
            {
                // Subscribe to property changes to update Global Checkbox & Command
                f.PropertyChanged += (s, e) => { 
                    if (e.PropertyName == nameof(Factura.Seleccionada)) 
                        CheckGlobalSelectionStates(); 
                };
                Facturas.Add(f);
            }

            Direcciones.Clear();
            foreach (var d in dirs) 
            {
                // Subscribe to property changes to update Global Checkbox & Command
                d.PropertyChanged += (s, e) => { 
                    if (e.PropertyName == nameof(DireccionEmail.Seleccionada)) 
                        CheckGlobalSelectionStates(); 
                };
                Direcciones.Add(d);
            }

            CheckGlobalSelectionStates();

            ResultadosVisibles = true;
            MensajeEstado = $"{Facturas.Count} facturas encontradas · {Direcciones.Count} direcciones de envío";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }

    private bool CanProcesar() => !Ocupado;

    [RelayCommand(CanExecute = nameof(CanEnviar))]
    private async Task EnviarMailAsync()
    {
        Ocupado = true;
        MensajeEstado = "Enviando correo...";

        try
        {
            var selFact = Facturas.Where(f => f.Seleccionada).ToList();
            var selDirs = Direcciones.Where(d => d.Seleccionada).ToList();

            await _emailService.EnviarMailAsync(EmailForm.AsuntoEmail, EmailForm.CuerpoEmail, selDirs, selFact);

            MensajeEstado = $"✓ Mail enviado correctamente a {selDirs.Count} dirección(es) con {selFact.Count} factura(s).";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al enviar: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }

    private bool CanEnviar() => !Ocupado && ResultadosVisibles && Direcciones.Any(d => d.Seleccionada);

    [RelayCommand]
    private void LimpiarCriterios()
    {
        ClienteDesde = 0;
        ClienteHasta = 0;
        FacturaDesde = 0;
        FacturaHasta = 0;
        MesAnio = DateTime.Now.ToString("MM-yyyy");
        SoloActuales = true;

        Facturas.Clear();
        Direcciones.Clear();
        ResultadosVisibles = false;
        MensajeEstado = string.Empty;
    }

    [RelayCommand]
    private void EliminarDireccion(DireccionEmail? dir)
    {
        if (dir is not null) Direcciones.Remove(dir);
    }
}
