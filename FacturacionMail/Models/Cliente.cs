using CommunityToolkit.Mvvm.ComponentModel;

namespace FacturacionMail.Models;

public partial class Cliente : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public int Codigo { get; set; }
}
