using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ScheduleEditor.Converters;

public static class ObjectConverters
{
    public static readonly IValueConverter IsNull = new FuncValueConverter<object?, bool>(x => x == null);
    public static readonly IValueConverter IsNotNull = new FuncValueConverter<object?, bool>(x => x != null);
}

public static class BoolConverters
{
    public static readonly IValueConverter Not = new FuncValueConverter<bool, bool>(x => !x);
    public static readonly IMultiValueConverter And = new BooleanAndConverter();
}

public class BooleanAndConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values?.All(v => v is bool b && b) == true;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

public class BooleanToActionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue.ToLower() switch
            {
                "true" => "Turn On",
                "false" => "Turn Off",
                _ => stringValue
            };
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue switch
            {
                "Turn On" => "true",
                "Turn Off" => "false",
                _ => stringValue
            };
        }
        return value?.ToString();
    }
}

public class BoolToCheckmarkConverter : IValueConverter
{
    public static readonly BoolToCheckmarkConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
            return "✓";
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}