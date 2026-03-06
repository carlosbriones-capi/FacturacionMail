using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Models;
using FacturacionMail.Services;
using FacturacionMail.Interfaces;
using FacturacionMail.Data;

namespace FacturacionMail.ViewModels;

public partial class FacturacionMailViewModel : ViewModelBase
{
    private readonly IFacturaService _facturaService;
    private readonly IEmailService _emailService;

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

    [ObservableProperty]
    private bool _seleccionarTodo = true;

    public ObservableCollection<ClienteSeleccionable> Clientes { get; } = new();
    
    public ICollectionView ClientesView { get; }

    public FacturacionMailViewModel(IFacturaService facturaService, IEmailService emailService)
    {
        _facturaService = facturaService;
        _emailService = emailService;

        ClientesView = CollectionViewSource.GetDefaultView(Clientes);
        ClientesView.Filter = FiltrarCliente;

        _ = CargarClientesAsync();
    }

    partial void OnSearchTermChanged(string value)
    {
        ClientesView.Refresh();
    }

    partial void OnSeleccionarTodoChanged(bool value)
    {
        if (value) MarcarTodos();
        else DesmarcarTodos();
    }

    private bool FiltrarCliente(object obj)
    {
        if (obj is ClienteSeleccionable cs)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
            return cs.Cliente.Codigo.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                   cs.Cliente.Nombre.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    [RelayCommand]
    private async Task CargarClientesAsync()
    {
        Ocupado = true;
        try
        {
            var clientes = await _facturaService.ObtenerClientesAsync();
            Clientes.Clear();
            foreach (var c in clientes)
            {
                // En modo mensual (no detallado) seleccionamos por defecto
                Clientes.Add(new ClienteSeleccionable(c, !IsDetailedMode));
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
    private void MarcarTodos()
    {
        foreach (ClienteSeleccionable c in ClientesView) c.IsSelected = true;
    }

    [RelayCommand]
    private void DesmarcarTodos()
    {
        foreach (ClienteSeleccionable c in ClientesView) c.IsSelected = false;
    }

    [RelayCommand]
    private void InicializaCriterios()
    {
        DesmarcarTodos();
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
            MensajeEstado = "Debes seleccionar al menos un cliente.";
            return;
        }

        Ocupado = true;
        MensajeEstado = "Enviando correos...";
        
        try
        {
            // Simulación de envío a cada cliente seleccionado
            // En una implementación real, esto podría requerir obtener facturas para cada uno.
            await Task.Delay(1000); 
            MensajeEstado = $"✓ Envío completado para {seleccionados.Count} clientes.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error en el envío: {ex.Message}";
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
    }
}
