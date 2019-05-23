using System; 
using System.Windows.Data;

namespace WatchCake.Windows.ValueConverters
{
    /// <summary>
    /// Converts decimal deviation value to an according arrow symbol.
    /// </summary>
    public class PricesToDynamicsIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strResult = "";

            decimal? input = value as decimal?;

            if (input == null)
                return strResult;

            double shift = (double)(decimal)input;

            if (shift > .1)
            {
                strResult = shift >= .15 ? "⬈" : "↗";
            }
            else if (shift < -0.01)
            {
                strResult = shift <= -0.15 ? "▼" : "↓";
            }
            else
                strResult = "-";

            return strResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}