using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Models;
using FacturacionMail.Services;
using FacturacionMail.Interfaces;
using System.ComponentModel;
using System.Windows.Data;

namespace FacturacionMail.ViewModels;

public partial class EnvioFacturasPendientesViewModel : ViewModelBase
{
    private readonly IClienteService _clienteService;
    private readonly IFacturaService _facturaService;
    private readonly IEmailService   _emailService;

    // ── Colecciones ───────────────────────────────────────────────────

    public ObservableCollection<Cliente> Clientes { get; } = [];
    public ICollectionView ClientesView { get; }
    public ObservableCollection<ListaEmail> Listas { get; } = [];
    public ObservableCollection<DireccionEmail> Direcciones { get; } = [];
    public ObservableCollection<Factura> Facturas { get; } = [];

    // ── Selección ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnviarMailCommand))]
    private Cliente? _clienteSeleccionado;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnviarMailCommand))]
    private ListaEmail? _listaSeleccionada;

    // ── Estado UI ─────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _listasVisibles = false;

    [ObservableProperty]
    private bool _detalleVisible = false;

    [ObservableProperty]
    private bool _seleccionarTodasDirecciones = true;

    [ObservableProperty]
    private bool _seleccionarTodasFacturas = true;

    [ObservableProperty]
    private string _filtroTexto = string.Empty;

    [ObservableProperty]
    private bool _isFiltroAbierto = false;

    private bool _isUpdatingSelection = false;

    // ── Email ─────────────────────────────────────────────────────────

    public EmailFormViewModel EmailForm { get; }

    // ── Constructor ───────────────────────────────────────────────────

    public EnvioFacturasPendientesViewModel(IClienteService clienteService, IFacturaService facturaService, IEmailService emailService)
    {
        _clienteService = clienteService;
        _facturaService = facturaService;
        _emailService   = emailService;

        EmailForm = new EmailFormViewModel(
            "Facturas pendientes — CM Capital Markets",
            "CM Capital Markets\nOchandiano, 2\n28023 Madrid\nTelf: +34 91 509 62 61\nE-mail: facturas@capi.es",
            OnInsertarDireccion);

        // Configurar la vista filtrada
        ClientesView = CollectionViewSource.GetDefaultView(Clientes);
        ClientesView.Filter = (obj) =>
        {
            if (string.IsNullOrWhiteSpace(FiltroTexto)) return true;
            if (obj is not Cliente c) return false;

            // Filtrar por código (comienzo del string) o nombre (contiene)
            string search = FiltroTexto.ToLower();
            return c.Codigo.ToString().Contains(search);
        };

        _ = CargarClientesAsync();
    }

    protected override void OnOcupadoChangedVirtual(bool value)
    {
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    partial void OnFiltroTextoChanged(string value)
    {
        ClientesView.Refresh();
        if (!string.IsNullOrEmpty(value))
        {
            IsFiltroAbierto = true;
        }
    }

    partial void OnSeleccionarTodasDireccionesChanged(bool value)
    {
        if (_isUpdatingSelection) return;
        _isUpdatingSelection = true;
        foreach (var d in Direcciones) d.Seleccionada = value;
        _isUpdatingSelection = false;
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    partial void OnSeleccionarTodasFacturasChanged(bool value)
    {
        if (_isUpdatingSelection) return;
        _isUpdatingSelection = true;
        foreach (var f in Facturas) f.Seleccionada = value;
        _isUpdatingSelection = false;
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    private void OnInsertarDireccion(string direccion)
    {
        Direcciones.Add(new DireccionEmail
        {
            Id = Direcciones.Count + 1000,
            Email = direccion,
            Nombre = "Manual",
            Seleccionada = true,
            CodigoCliente = ClienteSeleccionado?.Codigo ?? 0
        });
        EnviarMailCommand.NotifyCanExecuteChanged();
    }

    // ── Reactive handlers ─────────────────────────────────────────────

    partial void OnClienteSeleccionadoChanged(Cliente? value)
    {
        Listas.Clear();
        Direcciones.Clear();
        Facturas.Clear();
        DetalleVisible  = false;
        ListasVisibles  = false;
        MensajeEstado   = string.Empty;
        ListaSeleccionada = null;

        if (value is not null)
            _ = CargarListasAsync(value.Codigo);
    }

    partial void OnListaSeleccionadaChanged(ListaEmail? value)
    {
        Direcciones.Clear();
        Facturas.Clear();
        DetalleVisible = false;
        MensajeEstado  = string.Empty;

        if (value is not null)
            _ = CargarDetalleAsync(value.Id);
    }

    // ── Comandos privados async ────────────────────────────────────────

    private async Task CargarClientesAsync()
    {
        Ocupado = true;
        try
        {
            var clientes = await _clienteService.ObtenerClientesAsync();
            var excluidos = await _clienteService.ObtenerClientesExcluidosAsync();
            var setExcluidos = new HashSet<string>(excluidos);

            Clientes.Clear();
            // Mostrar solo clientes únicos (por Codigo) y NO excluidos para el combo
            foreach (var c in clientes.DistinctBy(c => c.Codigo))
            {
                if (!setExcluidos.Contains(c.Codigo.ToString()))
                    Clientes.Add(c);
            }
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar clientes: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }

    [RelayCommand]
    private async Task VisualizarFacturaAsync(Factura factura)
    {
        if (factura == null) return;
        try
        {
            await _facturaService.VisualizarFacturaAsync(factura.NombreArchivo);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error al abrir factura", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task CargarListasAsync(int codigoCliente)
    {
        Ocupado = true;
        try
        {
            var listas = await _emailService.ObtenerListasAsync(codigoCliente);
            Listas.Clear();
            foreach (var l in listas)
                Listas.Add(l);

            ListasVisibles = Listas.Any();

            if (!Listas.Any())
                MensajeEstado = "Este cliente no tiene listas de envío configuradas.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar listas: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }

    private async Task CargarDetalleAsync(int listaId)
    {
        Ocupado = true;
        try
        {
            var dirs     = await _emailService.ObtenerDireccionesPorListaAsync(listaId);
            var facturas = await _facturaService.ObtenerFacturasPendientesPorListaAsync(listaId);

            Direcciones.Clear();
            foreach (var d in dirs) Direcciones.Add(d);

            Facturas.Clear();
            foreach (var f in facturas) Facturas.Add(f);

            DetalleVisible  = true;
            MensajeEstado   = $"{Facturas.Count} factura(s) pendiente(s) · {Direcciones.Count} destinatario(s)";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar detalle: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }

    // ── Comandos ──────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanEnviar))]
    private async Task EnviarMailAsync()
    {
        Ocupado = true;
        MensajeEstado = "Enviando correo...";
        try
        {
            var selDirs  = Direcciones.Where(d => d.Seleccionada).ToList();
            var selFact  = Facturas.Where(f => f.Seleccionada).ToList();

            if (!selDirs.Any())
            {
                MensajeEstado = "Selecciona al menos un destinatario.";
                return;
            }
            if (!selFact.Any())
            {
                MensajeEstado = "Selecciona al menos una factura.";
                return;
            }

            await _emailService.EnviarMailAsync(EmailForm.AsuntoEmail, EmailForm.CuerpoEmail, selDirs, selFact);
            MensajeEstado = $"✓ Correo enviado a {selDirs.Count} destinatario(s) con {selFact.Count} factura(s).";
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

    private bool CanEnviar() =>
        !Ocupado && DetalleVisible &&
        ClienteSeleccionado is not null &&
        Direcciones.Any(d => d.Seleccionada);

    [RelayCommand]
    private void EliminarDireccion(DireccionEmail? dir)
    {
        if (dir is not null) Direcciones.Remove(dir);
    }

    [RelayCommand]
    private void Limpiar()
    {
        ClienteSeleccionado = null;
        ListaSeleccionada   = null;
        Listas.Clear();
        Direcciones.Clear();
        Facturas.Clear();
        ListasVisibles  = false;
        DetalleVisible  = false;
        MensajeEstado   = string.Empty;
    }
}
