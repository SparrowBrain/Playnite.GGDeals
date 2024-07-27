using System;
using System.Globalization;
using System.Windows.Data;
using GGDeals.Services;
using Playnite.SDK;

namespace GGDeals.Infrastructure.Converters
{
    internal class AddResultToLocalizedStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AddToCollectionResult result)
            {
                switch (result)
                {
                    case AddToCollectionResult.NotFound:
                        return ResourceProvider.GetString("LOC_GGDeals_ShowAddFailuresReasonPageNotFound");

                    case AddToCollectionResult.New:
                        return ResourceProvider.GetString("LOC_GGDeals_ShowAddFailuresReasonNotProcessed");

                    default:
                        return result.ToString();
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}