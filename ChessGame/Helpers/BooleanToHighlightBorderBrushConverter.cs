using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChessGame.Helpers
{
    public class BooleanToHighlightBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHighlighted && isHighlighted)
            {
                return Brushes.Yellow;  // Highlight color
            }
            return Brushes.Transparent;  // Default border color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
