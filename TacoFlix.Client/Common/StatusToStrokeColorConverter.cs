using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Sitecore.Processing.Tasks.Abstractions;

namespace TacoFlix.Client.Common
{
    class StatusToStrokeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color fillColor = Colors.DarkGray;

            if (value != null)
            {
                switch ((ProcessingTaskStatus)value)
                {
                    case ProcessingTaskStatus.Pending:
                        fillColor = Colors.DarkGray;
                        break;
                    case ProcessingTaskStatus.Processing:
                        fillColor = Colors.DarkGoldenrod;
                        break;
                    case ProcessingTaskStatus.Completed:
                        fillColor = Colors.DarkGreen;
                        break;
                    case ProcessingTaskStatus.Failed:
                        fillColor = Colors.DarkRed;
                        break;
                }
            }

            return new SolidColorBrush(fillColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
