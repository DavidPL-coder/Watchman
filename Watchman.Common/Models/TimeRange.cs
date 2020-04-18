﻿using System;

namespace Watchman.Common.Models
{
    public class TimeRange
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public int MinutesBetween => (int)(End - Start).TotalMinutes;
        public int HoursBetween => (int)(End - Start).TotalHours;
        public int DaysBetween => (int)(End - Start).TotalDays;

        public TimeRange()
        {
        }

        public TimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public static TimeRange Create(DateTime start, DateTime end) => new TimeRange(start, end);
        public static TimeRange ToNow(DateTime start) => new TimeRange(start, DateTime.UtcNow);
        public static TimeRange FromNow(DateTime end) => new TimeRange(DateTime.UtcNow, end);

        public void ForeachMinute(Action<int, DateTime> action) => this.Foreach(this.MinutesBetween, Start.AddMinutes, action);
        public void ForeachHour(Action<int, DateTime> action) => this.Foreach(this.HoursBetween, Start.AddHours, action);
        public void ForeachDay(Action<int, DateTime> action) => this.Foreach(this.DaysBetween, Start.AddDays, action);

        private void Foreach(int loop, Func<double, DateTime> add, Action<int, DateTime> action)
        {
            for (int i = 0; i < loop; i++)
            {
                action.Invoke(i, add.Invoke(i));
            }
        }
    }
}
