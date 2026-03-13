using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacturacionMail.Models;
using System.Threading.Tasks;
using System;

namespace FacturacionMail.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _ocupado = false;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    private bool _isModalVisible = false;

    [ObservableProperty]
    private ModalType _modalType = ModalType.None;

    [ObservableProperty]
    private string _modalTitle = string.Empty;

    [ObservableProperty]
    private string _modalMessage = string.Empty;

    protected void ShowLoading(string message = "Procesando, por favor espere...", string title = "Cargando")
    {
        ModalType = ModalType.Loading;
        ModalTitle = title;
        ModalMessage = message;
        IsModalVisible = true;
        Ocupado = true;
    }

    protected async void ShowSuccess(string message, string title = "¡Completado!")
    {
        ModalType = ModalType.Success;
        ModalTitle = title;
        ModalMessage = message;
        IsModalVisible = true;
        Ocupado = false;
        
        await Task.Delay(2500);
        if (ModalType == ModalType.Success)
        {
            IsModalVisible = false;
        }
    }

    protected void ShowError(string message, string title = "Error")
    {
        ModalType = ModalType.Error;
        ModalTitle = title;
        ModalMessage = message;
        IsModalVisible = true;
        Ocupado = false;
    }

    protected void HideModal()
    {
        IsModalVisible = false;
        Ocupado = false;
    }

    [RelayCommand]
    public void CerrarModal() => IsModalVisible = false;

    partial void OnOcupadoChanged(bool value)
    {
        OnOcupadoChangedVirtual(value);
    }

    protected virtual void OnOcupadoChangedVirtual(bool value)
    {
    }
}
