using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _ocupado = false;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    partial void OnOcupadoChanged(bool value)
    {
        OnOcupadoChangedVirtual(value);
    }

    protected virtual void OnOcupadoChangedVirtual(bool value)
    {
    }
}
