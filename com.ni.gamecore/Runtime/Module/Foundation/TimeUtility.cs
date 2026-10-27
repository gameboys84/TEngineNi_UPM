using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tap4fun {

    public static class TimeUtility {
        private readonly static DateTime _UnixOriginTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ConvertFromUnixTimestamp(double timestamp) {
            DateTime origin = _UnixOriginTime;
            return origin.AddSeconds(timestamp);
        }

        public static DateTime ConvertFromUnixTimestampMsec(double timestamp) {
            DateTime origin = _UnixOriginTime;
            return origin.AddMilliseconds(timestamp);
        }

        public static DateTime ConvertToCustomLocalTime(DateTime time, int currentTimeZone) {
            return time.AddSeconds(currentTimeZone * 60.0f * 60.0f);
        }

        public static DateTime ConvertServerTimestampToLocalDateTime(double timeSpan) {
            DateTime utcDate = _UnixOriginTime.AddSeconds(timeSpan);
            return TimeZone.CurrentTimeZone.ToLocalTime(utcDate);
        }

        public static DateTime ConvertFromStringWithSplit(string timeStr, string split) {
            string[] splitedTimeStrs = timeStr.Split(split.ToCharArray());
            if (splitedTimeStrs.Length != 6) {
                Debug.LogError("wrong time string format or split");
                return DateTime.MinValue;
            }

            int year = Convert.ToInt32(splitedTimeStrs[0]);
            int month = Convert.ToInt32(splitedTimeStrs[1]);
            int day = Convert.ToInt32(splitedTimeStrs[2]);
            int hour = Convert.ToInt32(splitedTimeStrs[3]);
            int minute = Convert.ToInt32(splitedTimeStrs[4]);
            int second = Convert.ToInt32(splitedTimeStrs[5]);
            DateTime dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            return dt;
        }

        public static string GetLocalDateStrWithSplit() {
            DateTime curDataTime = DateTime.Now;

            int year = curDataTime.Year;
            int month = curDataTime.Month;
            int day = curDataTime.Day;
            int hour = curDataTime.Hour;
            int minute = curDataTime.Minute;
            int second = curDataTime.Second;

            return string.Format("{0},{1},{2},{3},{4},{5}",year, month, day, hour, minute, second);
        }

        public static string GetLocalDateStr() {
            DateTime curDataTime = DateTime.Now;
            return curDataTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static double ConvertToUnixTimestamp(DateTime date) {
            DateTime origin = _UnixOriginTime;
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static bool IsBetweenTime(DateTime currentTime, DateTime beginTime, DateTime endTime) {
            if (currentTime.CompareTo(beginTime) >= 0
            && currentTime.CompareTo(endTime) <= 0) {
                return true;
            }
            return false;
        }
    }
}
