﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sop.Data.Utility;

namespace Sop.Core.Utility
{
    public static class JsonUtility
    {
        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="dateTimeType">Type of the date time.</param>
        /// <param name="dateFormatString">The date format string.</param>
        /// <returns></returns>
        public static string ToJson(this object obj, DateTimeType dateTimeType, string dateFormatString = "yyyy-MM-dd HH:mm:ss fff")
        {

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new SopJsonResolver(),
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                NullValueHandling = NullValueHandling.Ignore
            };
            switch (dateTimeType)
            {
                case DateTimeType.MicrosoftDateFormatUtc:
                    settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    break;
                case DateTimeType.Default:
                    if (string.IsNullOrEmpty(dateFormatString))
                    {
                        dateFormatString = "yyyy-MM-dd HH:mm:ss fff";
                    }
                    settings.DateFormatString = dateFormatString;
                    break;
                default:

                    break;

            }
            var str = JsonConvert.SerializeObject(obj, Formatting.None, settings);
            return str;

        }


        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string ToJson(this object obj, PropertyNameType type = PropertyNameType.ToLower)
        {
            var settings = new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateFormatString = "yyyy-MM-dd hh:mm:ss fff",
                DateTimeZoneHandling = DateTimeZoneHandling.Local

            };
            switch (type)
            {
                case PropertyNameType.CamelCase:
                    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    break;
                case PropertyNameType.ToLower:
                case PropertyNameType.ToUpper:
                case PropertyNameType.Default:
                default:
                    settings.ContractResolver = new SopJsonResolver(type);
                    break;
            }

            return JsonConvert.SerializeObject(obj, Formatting.None, settings);

        }

        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public static string ToJson(this object obj, object settings)
        {
            var settings2 = settings as JsonSerializerSettings;
            return JsonConvert.SerializeObject(obj, settings2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json,
                new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss fff" });

        }

        public static T FromJson<T>(this string json, object settings)
        {
            var settings2 = settings as JsonSerializerSettings;
            return JsonConvert.DeserializeObject<T>(json, settings2);

        }



    }
}
