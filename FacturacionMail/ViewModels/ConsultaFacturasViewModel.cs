using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FacturacionMail.Data;
using FacturacionMail.Models;
using FacturacionMail.Services;

namespace FacturacionMail.ViewModels;

public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class ConsultaFacturasViewModel : INotifyPropertyChanged
{
    private readonly IFacturaService _facturaService;
    private readonly IEmailService   _emailService;

    // ── Criterios de selección ───────────────────────────────────────

    private int _clienteDesde;
    private int _clienteHasta;
    private int _facturaDesde;
    private int _facturaHasta;
    private string _mesAnio = DateTime.Now.ToString("MM-yyyy");
    private bool _soloActuales = true;

    public int ClienteDesde
    {
        get => _clienteDesde;
        set { _clienteDesde = value; OnPropertyChanged(); }
    }
    public int ClienteHasta
    {
        get => _clienteHasta;
        set { _clienteHasta = value; OnPropertyChanged(); }
    }
    public int FacturaDesde
    {
        get => _facturaDesde;
        set { _facturaDesde = value; OnPropertyChanged(); }
    }
    public int FacturaHasta
    {
        get => _facturaHasta;
        set { _facturaHasta = value; OnPropertyChanged(); }
    }
    public string MesAnio
    {
        get => _mesAnio;
        set { _mesAnio = value; OnPropertyChanged(); }
    }
    public bool SoloActuales
    {
        get => _soloActuales;
        set { _soloActuales = value; OnPropertyChanged(); }
    }

    // ── Email ────────────────────────────────────────────────────────

    private string _asuntoEmail = "Facturas C.M. Capital Markets";
    private string _cuerpoEmail = "CM Capital Markets\nOchandiano, 2\n28023 Madrid\nTelf: +34 91 509 62 61\nE-mail: facturas@capi.es";
    private string _nuevaDireccion = string.Empty;

    public string AsuntoEmail
    {
        get => _asuntoEmail;
        set { _asuntoEmail = value; OnPropertyChanged(); }
    }
    public string CuerpoEmail
    {
        get => _cuerpoEmail;
        set { _cuerpoEmail = value; OnPropertyChanged(); }
    }
    public string NuevaDireccion
    {
        get => _nuevaDireccion;
        set { _nuevaDireccion = value; OnPropertyChanged(); }
    }

    // ── Estado UI ────────────────────────────────────────────────────

    private bool _resultadosVisibles = false;
    private bool _ocupado            = false;
    private string _mensajeEstado    = string.Empty;

    public bool ResultadosVisibles
    {
        get => _resultadosVisibles;
        set { _resultadosVisibles = value; OnPropertyChanged(); }
    }
    public bool Ocupado
    {
        get => _ocupado;
        set { _ocupado = value; OnPropertyChanged(); }
    }
    public string MensajeEstado
    {
        get => _mensajeEstado;
        set { _mensajeEstado = value; OnPropertyChanged(); }
    }

    // ── Colecciones ───────────────────────────────────────────────────

    public ObservableCollection<Factura>       Facturas    { get; } = [];
    public ObservableCollection<DireccionEmail> Direcciones { get; } = [];

    // ── Comandos ──────────────────────────────────────────────────────

    public ICommand ProcesarSeleccionCommand { get; }
    public ICommand EnviarMailCommand        { get; }
    public ICommand LimpiarCriteriosCommand  { get; }
    public ICommand InsertarDireccionCommand { get; }
    public ICommand EliminarDireccionCommand { get; }

    public ConsultaFacturasViewModel()
    {
        _facturaService = new MockDataService();
        _emailService   = (IEmailService)_facturaService;

        ProcesarSeleccionCommand = new RelayCommand(async _ => await ProcesarSeleccionAsync(), _ => !Ocupado);
        EnviarMailCommand        = new RelayCommand(async _ => await EnviarMailAsync(),
                                                    _ => !Ocupado && ResultadosVisibles && Direcciones.Any(d => d.Seleccionada));
        LimpiarCriteriosCommand  = new RelayCommand(_ => LimpiarCriterios());
        InsertarDireccionCommand = new RelayCommand(_ => InsertarDireccion(), _ => !string.IsNullOrWhiteSpace(NuevaDireccion));
        EliminarDireccionCommand = new RelayCommand(param => EliminarDireccion(param as DireccionEmail));
    }

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
            foreach (var f in facturas) Facturas.Add(f);

            Direcciones.Clear();
            foreach (var d in dirs) Direcciones.Add(d);

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

    private async Task EnviarMailAsync()
    {
        Ocupado = true;
        MensajeEstado = "Enviando correo...";

        try
        {
            var selFact = Facturas.Where(f => f.Seleccionada).ToList();
            var selDirs = Direcciones.Where(d => d.Seleccionada).ToList();

            await _emailService.EnviarMailAsync(AsuntoEmail, CuerpoEmail, selDirs, selFact);

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

    private void LimpiarCriterios()
    {
        ClienteDesde    = 0;
        ClienteHasta    = 0;
        FacturaDesde    = 0;
        FacturaHasta    = 0;
        MesAnio         = DateTime.Now.ToString("MM-yyyy");
        SoloActuales    = true;

        Facturas.Clear();
        Direcciones.Clear();
        ResultadosVisibles = false;
        MensajeEstado      = string.Empty;
    }

    private void InsertarDireccion()
    {
        if (string.IsNullOrWhiteSpace(NuevaDireccion)) return;
        Direcciones.Add(new DireccionEmail
        {
            Id    = Direcciones.Count + 100,
            Email = NuevaDireccion.Trim(),
            Nombre = "Manual",
            Seleccionada = true
        });
        NuevaDireccion = string.Empty;
    }

    private void EliminarDireccion(DireccionEmail? dir)
    {
        if (dir is not null) Direcciones.Remove(dir);
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
