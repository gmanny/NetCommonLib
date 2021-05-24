using Newtonsoft.Json.Converters;

namespace Monitor.ServiceCommon.Services.Json
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}