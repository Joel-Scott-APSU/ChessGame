using System.Globalization;
using System.Windows.Data;

namespace ChessGame
{
    public class PositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int position && parameter is string direction)
            {
                int row = position / 8;
                int column = position % 8;

                // Adjust row calculation to reflect bottom-left origin
                return direction == "Row" ? 7 - row : column;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


