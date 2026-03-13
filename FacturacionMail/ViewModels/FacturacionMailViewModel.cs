using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Models;
using FacturacionMail.Services;
using FacturacionMail.Interfaces;

namespace FacturacionMail.ViewModels;

public partial class FacturacionMailViewModel : ViewModelBase
{
    private readonly IClienteService _clienteService;
    private readonly IFacturaService _facturaService;
    private readonly IEmailService _emailService;
    private readonly IAppLogger _logger;

    [ObservableProperty]
    private string _asuntoEmail = "Facturacion CM.Capital Markets mar-2026";

    [ObservableProperty]
    private string _cuerpoEmailMensual = "CM Capital Markets\nOchandiano, 2\n28023 Madrid\nTelf: +34 91 509 62 61\nE-mail: facturas@capi.es\n\n" +
        "Aviso de Confidencialidad: Esta transmisión se entiende para uso del destinatario o la entidad a la que va dirigida y puede contener información confidencial o protegida por la ley. " +
        "Si el lector de este mensaje no fuera el destinatario, considérese por este medio informado de que la retención, difusión, o copia de este correo electrónico está estrictamente prohibida. " +
        "Si recibe este mensaje por error, notifíquelo, por favor, al emisor inmediatamente por teléfono y destruya el original. Gracias.\n\n" +
        "Confidentiality Notice: This transmission is intended for the use of the individual or entity to which it is addressed, and it may contain information that is confidential or privileged under law. " +
        "If the reader of this message is not the intended recipient, you are hereby notified that retention, dissemination, distribution or copying of this e-mail is strictly prohibited. " +
        "If you received this e-mail in error, please notify the sender immediately by telephone and destroy the original. Thank you.";

    [ObservableProperty]
    private string _cuerpoEmailDetallado = "CM Capital Markets\nOchandiano, 2\n28023 Madrid\nTelf: +34 91 509 62 61\nE-mail: facturas@capi.es";

    [ObservableProperty]
    private string _mesAnio = DateTime.Now.ToString("MM-yyyy");

    [ObservableProperty]
    private bool _isDetailedMode;

    [ObservableProperty]
    private string _searchTerm = string.Empty;


    public string CuerpoEmailActual
    {
        get => IsDetailedMode ? CuerpoEmailDetallado : CuerpoEmailMensual;
        set
        {
            if (IsDetailedMode) CuerpoEmailDetallado = value;
            else CuerpoEmailMensual = value;
            OnPropertyChanged(nameof(CuerpoEmailActual));
        }
    }

    public ObservableCollection<Cliente> Clientes { get; } = new();
    
    public ICollectionView ClientesView { get; }

    public FacturacionMailViewModel(IClienteService clienteService, IFacturaService facturaService, IEmailService emailService, IAppLogger logger)
    {
        _clienteService = clienteService;
        _facturaService = facturaService;
        _emailService = emailService;
        _logger = logger;

        ClientesView = CollectionViewSource.GetDefaultView(Clientes);
        ClientesView.Filter = FiltrarCliente;

        _ = CargarClientesAsync();
    }

    partial void OnSearchTermChanged(string value)
    {
        ClientesView.Refresh();
    }


    private bool FiltrarCliente(object obj)
    {
        if (obj is Cliente c)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
            return c.Codigo.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    [RelayCommand]
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
                {
                    c.IsSelected = !IsDetailedMode;
                    Clientes.Add(c);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error al cargar clientes: {ex.Message}");
        }
        finally
        {
            Ocupado = false;
        }
    }


    [RelayCommand]
    private void InicializaCriterios()
    {
        foreach (Cliente c in Clientes) c.IsSelected = false;
        AsuntoEmail = "Facturacion CM.Capital Markets";
        CuerpoEmailMensual = "CM Capital Markets...";
        CuerpoEmailDetallado = "CM Capital Markets...";
    }

    [RelayCommand]
    private async Task EnviarMailAsync()
    {
        var seleccionados = Clientes.Where(c => c.IsSelected).ToList();
        if (!seleccionados.Any())
        {
            ShowError("Casi lo olvidas: debes seleccionar al menos un cliente para poder enviar las facturas.", "Falta Selección");
            return;
        }

        ShowLoading("Registrando envíos...", "Enviando Correo(s)");
        
        try
        {
            _logger.ToLog($"[USER ACTION] Inicio de proceso de envío masivo para {seleccionados.Count} clientes.");
            int enviosRealizados = 0;
            foreach (var cliente in seleccionados)
            {
                var facturas = await _facturaService.ObtenerFacturasAsync(MesAnio, cliente.Codigo, cliente.Codigo, 0, 0, true);
                var listaFacturas = facturas.ToList();

                if (!listaFacturas.Any()) continue;

                var grupos = listaFacturas.GroupBy(f => f.ListaId);
                foreach (var grupo in grupos)
                {
                    var direcciones = await _emailService.ObtenerDireccionesPorListaAsync(grupo.Key);
                    var listaDirecciones = direcciones.ToList();

                    if (listaDirecciones.Any())
                    {
                        await _emailService.EnviarMailAsync(AsuntoEmail, CuerpoEmailActual, listaDirecciones, grupo);
                        enviosRealizados++;
                    }
                }
            }
            
            ShowSuccess($"Se han registrado {enviosRealizados} órdenes de envío en el historial.", "Envío Finalizado");
        }
        catch (Exception ex)
        {
            ShowError($"Error en el envío: {ex.Message}");
        }
        finally
        {
            Ocupado = false;
        }
    }

    partial void OnIsDetailedModeChanged(bool value)
    {
        // Al cambiar de modo, reiniciamos la selección según corresponda
        foreach (var c in Clientes)
        {
            c.IsSelected = !value;
        }
        OnPropertyChanged(nameof(CuerpoEmailActual));
    }
}
