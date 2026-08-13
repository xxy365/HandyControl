using System;
using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class NumberPropertyEditor : PropertyEditorBase
{
    public NumberPropertyEditor()
    {
    }

    public NumberPropertyEditor(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public double Minimum { get; set; }

    public double Maximum { get; set; }

    public override Control CreateElement(PropertyItem propertyItem)
    {
        var numericUpDown = new NumericUpDown
        {
            IsReadOnly = propertyItem.IsReadOnly,
            Minimum = ToDecimal(Minimum),
            Maximum = ToDecimal(Maximum)
        };

        return numericUpDown;
    }

    public override AvaloniaProperty GetAvaloniaProperty() => NumericUpDown.ValueProperty;

    private static decimal ToDecimal(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return decimal.Zero;
        }

        try
        {
            return (decimal)value;
        }
        catch (OverflowException)
        {
            return value > 0 ? decimal.MaxValue : decimal.MinValue;
        }
    }
}
