//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.Xaml.Data;

namespace Phrasor
{
    public class TimeSpanToMillisecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan ts = (TimeSpan)value;
            double result = ts.TotalMilliseconds;
            return result.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int totalMilliseconds = int.Parse(value.ToString());           
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, totalMilliseconds);           
            return ts;            
        }
    }

}
