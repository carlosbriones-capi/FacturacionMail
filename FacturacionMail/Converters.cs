using System;
using System.Globalization;
using System.Windows.Data;

namespace FacturacionMail;

/// <summary>
/// Invierte un valor bool. Usado para binding de radio buttons (SoloActuales / Todas).
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBoolConverter : IValueConverter
{
    public static readonly InvertBoolConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : false;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

/// <summary>
/// Convierte un string no vacío a Visible, y vacío a Collapsed.
/// </summary>
[ValueConversion(typeof(string), typeof(System.Windows.Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public static readonly StringToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string)
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Invierte bool y lo convierte a Visibility.
/// False → Visible (usado para el estado vacío: cuando NO hay resultados).
/// True  → Collapsed.
/// </summary>
[ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
public class InvertBoolToVisibilityConverter : IValueConverter
{
    public static readonly InvertBoolToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BindingProxy : System.Windows.Freezable
{
    protected override System.Windows.Freezable CreateInstanceCore() => new BindingProxy();

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly System.Windows.DependencyProperty DataProperty =
        System.Windows.DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new System.Windows.PropertyMetadata(null));
}

/// <summary>
/// Convierte entre int y string, manejando valores vacíos como 0.
/// Evita errores de binding al borrar el contenido de un TextBox numérico.
/// </summary>
[ValueConversion(typeof(int), typeof(string))]
public class NumericStringConverter : IValueConverter
{
    public static readonly NumericStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i) return i == 0 ? string.Empty : i.ToString();
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return 0;
        if (int.TryParse(s, out int result)) return result;
        return 0;
    }
}


public class EqualityConverter : IMultiValueConverter
{
    public static readonly EqualityConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return false;
        if (values[0] == null || values[1] == null) return false;
        return values[0].ToString() == values[1].ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is System.Windows.Visibility v && v == System.Windows.Visibility.Visible;
}
