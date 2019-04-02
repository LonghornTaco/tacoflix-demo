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
    public class StatusToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color fillColor = Colors.LightGray;

            if (value != null)
            {
                switch ((ProcessingTaskStatus)value)
                {
                    case ProcessingTaskStatus.Pending:
                        fillColor = Colors.LightGray;
                        break;
                    case ProcessingTaskStatus.Processing:
                        fillColor = Colors.LightGoldenrodYellow;
                        break;
                    case ProcessingTaskStatus.Completed:
                        fillColor = Colors.LightGreen;
                        break;
                    case ProcessingTaskStatus.Failed:
                        fillColor = Colors.LightCoral;
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
