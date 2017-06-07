using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public static class UtilityHelper
    {
        public static T GetConfigValue<T>(string key)
        {
            var configValue = "Get From Config File";
            return String.IsNullOrEmpty(configValue) ? default(T) : (T)Convert.ChangeType(configValue, typeof(T));
        }

        public static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static int MinutesFromNow(this DateTime date)
        {
            TimeSpan timeSpan = date - DateTime.UtcNow;
            return Convert.ToInt32(Math.Round(timeSpan.TotalMinutes));
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());

            DescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return attribs.Length > 0 ? attribs[0].Description : null;
        }

        public static Dictionary<string, string> GetDescriptionAndValue(TestClass testClass)
        {
            var result = new Dictionary<string, string>();

            var testType = testClass.GetType();
            foreach (var propertyInfo in testType.GetProperties())
            {
                foreach (var attribute in propertyInfo.GetCustomAttributes(false))
                {
                    var descriptionAttribute = attribute as DescriptionAttribute;
                    if (descriptionAttribute != null)
                        if (!result.ContainsKey(descriptionAttribute.Description))
                            result.Add(descriptionAttribute.Description, propertyInfo.GetValue(testClass, null).ToString());
                }
            }


            return result;
        }

        public static bool IsIn<T>(this T source, params T[] list)
        {
            if (null == source)
                throw new ArgumentException("source");

            return list.Contains(source);
        }

        public static T DeepCopy<T>(this T source)
        {
            if (null == source)
                throw new ArgumentException("source");

            //return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));

            return default(T);
        }

        public static byte[] SendPOX(string data, string url, string username, string password, bool keepAlive, long maxResponseStreamLength, int? timeoutMilliseconds)
        {
            byte[] result = null;
            byte[] buffer = new byte[4096];
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            if (!keepAlive)
                request.KeepAlive = false;

            if (timeoutMilliseconds != null)
                request.Timeout = timeoutMilliseconds.Value;

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                request.Credentials = new NetworkCredential(username, password);
                request.PreAuthenticate = true;
            }

            request.ContentType = "text/xml";
            request.ContentLength = bytes.Length;

            using (Stream stream = request.GetRequestStream())
                stream.Write(bytes, 0, bytes.Length);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.ContentLength > maxResponseStreamLength)
                    throw new Exception("");

                if (response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                count = responseStream.Read(buffer, 0, buffer.Length);
                                ms.Write(buffer, 0, count);
                            } while (count != 0);

                            result = ms.ToArray();
                        }
                    }
                }
            }

            return result;
        }
    }
}
