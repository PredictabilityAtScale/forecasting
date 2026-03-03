using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace FocusedObjective.KanbanSim
{
    [ValueConversion(typeof(string), typeof(double))]
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (s != null)
            {
                double result = 0.0;

                if (double.TryParse(s, out result))
                    return result;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return DependencyProperty.UnsetValue;
        }
    }
}
