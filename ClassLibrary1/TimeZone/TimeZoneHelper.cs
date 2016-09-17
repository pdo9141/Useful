using System;
using System.Linq;
using System.Collections.Generic;

namespace ClassLibrary1.TimeZone
{
    public static class TimeZoneHelper
    {
        private static Dictionary<string, string> _timeZoneList = null;

        public static Dictionary<string, string> GetTimeZoneList()
        {
            if (_timeZoneList == null && !String.IsNullOrEmpty("TimeZoneIdAppValue"))
            {
                _timeZoneList = "TimeZoneIdAppValue"
                    .Split(',')
                    .Select(e => e.Split('|'))
                    .ToDictionary(e => e[0].Trim().ToLower(), e => e[1]);
            }

            return _timeZoneList;
        }

        public static void CheckTimeZone()
        {
            string dateTimeString = "";
            string dateTimeZone = "";

            var timeZone = dateTimeZone.Trim().ToLower();
            var timeZoneList = GetTimeZoneList();
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneList[timeZone]);

            DateTime dateTimeToProcess = Convert.ToDateTime(dateTimeString);
            DateTime dateTimeToProcessUTC = TimeZoneInfo.ConvertTimeToUtc(dateTimeToProcess, timeZoneInfo);
        }
    }
}
