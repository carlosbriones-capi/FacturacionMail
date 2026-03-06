using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FacturacionMail.ViewModels;

public partial class EmailFormViewModel : ObservableObject
{
    private readonly Action<string> _onInsertarDireccion;

    [ObservableProperty]
    private string _asuntoEmail;

    [ObservableProperty]
    private string _cuerpoEmail;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InsertarDireccionCommand))]
    private string _nuevaDireccion = string.Empty;

    public EmailFormViewModel(string defaultAsunto, string defaultCuerpo, Action<string> onInsertarDireccion)
    {
        _asuntoEmail = defaultAsunto;
        _cuerpoEmail = defaultCuerpo;
        _onInsertarDireccion = onInsertarDireccion;
    }

    [RelayCommand(CanExecute = nameof(CanInsertarDireccion))]
    private void InsertarDireccion()
    {
        if (_onInsertarDireccion != null && !string.IsNullOrWhiteSpace(NuevaDireccion))
        {
            _onInsertarDireccion(NuevaDireccion.Trim());
            NuevaDireccion = string.Empty;
        }
    }

    private bool CanInsertarDireccion() => !string.IsNullOrWhiteSpace(NuevaDireccion);
}
