using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppCommon.Utils;

public enum ThicknessSide { Left, Top, Right, Bottom, LeftRight, TopBottom, All }

public class WidthToThicknessConverter : IValueConverter
{
    private ThicknessSide side;
    private bool invert;
    private double divide = 1;

    public WidthToThicknessConverter() { }

    public WidthToThicknessConverter(ThicknessSide side)
    {
        this.side = side;
    }

    public ThicknessSide Side
    {
        get => side;
        set => side = value;
    }

    public bool Invert
    {
        get => invert;
        set => invert = value;
    }

    public double Divide
    {
        get => divide;
        set => divide = value;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double width || !Double.IsFinite(width))
        {
            return new Thickness(0);
        }

        width /= divide;

        if (invert)
        {
            width = -width;
        }

        return side switch
        {
            ThicknessSide.Left => new Thickness(width, 0, 0, 0),
            ThicknessSide.Top => new Thickness(0, width, 0, 0),
            ThicknessSide.Right => new Thickness(0, 0, width, 0),
            ThicknessSide.Bottom => new Thickness(0, 0, 0, width),
            ThicknessSide.LeftRight => new Thickness(width, 0, width, 0),
            ThicknessSide.TopBottom => new Thickness(0, width, 0, width),
            ThicknessSide.All => new Thickness(width, width, width, width),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, "Side not recognized")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}