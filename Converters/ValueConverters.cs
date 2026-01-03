using mg3.foundry.Features.Chat;
using mg3.foundry.Features.FoundryCore.Services;
using System.Globalization;

namespace mg3.foundry.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Colors.Green : Colors.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBorderColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Colors.Blue : Colors.LightGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Colors.LightBlue : Colors.LightGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? LayoutOptions.End : LayoutOptions.Start;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToErrorColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? Colors.Red : Colors.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            var result = i > 0;
            if (parameter is string s && s.Equals("Inverted", StringComparison.OrdinalIgnoreCase))
                return !result;
            return result;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotificationTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => Colors.Blue,
                NotificationType.Success => Colors.Green,
                NotificationType.Warning => Colors.Orange,
                NotificationType.Error => Colors.Red,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotificationTypeToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => Color.FromArgb("#E3F2FD"),
                NotificationType.Success => Color.FromArgb("#E8F5E9"),
                NotificationType.Warning => Color.FromArgb("#FFF8E1"),
                NotificationType.Error => Color.FromArgb("#FFEBEE"),
                _ => Colors.White
            };
        }
        return Colors.White;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotificationTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "ℹ️",
                NotificationType.Success => "✅",
                NotificationType.Warning => "⚠️",
                NotificationType.Error => "❌",
                _ => "📌"
            };
        }
        return "📌";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isCached && isCached ? "Cached" : "Available";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AnyTrueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ChatViewModel viewModel)
        {
            return viewModel.AttachedImagePreview != null || viewModel.AttachedFileName != null;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
