﻿using Statsman.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watchman.Common.Models;
using Watchman.DomainModel.Messages;

namespace Statsman.Core.TimeSplitting
{
    public class TimeSplittingService
    {
        public IEnumerable<TimeStatisticItem> GetStatisticsPerQuarter(IEnumerable<ServerDayStatistic> serverDayStatistics, IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            var statisticsPerDay = this.GetStatisticsPerDay(serverDayStatistics, latestMessages, expectedTimeRange).ToList();
            var result = this.SumItems(statisticsPerDay, 90, expectedTimeRange);
            return result;
        }

        public IEnumerable<TimeStatisticItem> GetStatisticsPerMonth(IEnumerable<ServerDayStatistic> serverDayStatistics, IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            var statisticsPerDay = this.GetStatisticsPerDay(serverDayStatistics, latestMessages, expectedTimeRange).ToList();
            var result = this.SumItems(statisticsPerDay, 30, expectedTimeRange);
            return result;
        }

        public IEnumerable<TimeStatisticItem> GetStatisticsPerWeek(IEnumerable<ServerDayStatistic> serverDayStatistics, IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            var statisticsPerDay = this.GetStatisticsPerDay(serverDayStatistics, latestMessages, expectedTimeRange).ToList();
            var result = this.SumItems(statisticsPerDay, 7, expectedTimeRange);
            return result;
        }

        public IEnumerable<TimeStatisticItem> GetStatisticsPerDay(IEnumerable<ServerDayStatistic> serverDayStatistics, IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            latestMessages = this.FilterMessages(latestMessages, serverDayStatistics);
            var oldestLastMessagesDate = latestMessages.OrderBy(x => x.SentAt).First().SentAt;
            var result = new List<TimeStatisticItem>();
            expectedTimeRange.ForeachDay((i, day) => 
            {
                var sum = 0;
                if(day >= oldestLastMessagesDate.Date)
                {
                    sum += latestMessages.Where(x => x.SentAt.Date == day).Count();
                }
                sum += serverDayStatistics.Where(x => x.Date.Date == day).OrderBy(x => x.CreatedAt).FirstOrDefault()?.Count ?? 0;
                var item = new TimeStatisticItem(TimeRange.Create(day, day.AddDays(1).AddSeconds(-1)), sum);
                result.Add(item);
            });
            return result;
        }

        public IEnumerable<TimeStatisticItem> GetStatisticsPerHour(IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            var result = new List<TimeStatisticItem>();
            expectedTimeRange.ForeachHour((i, hour) => 
            {
                var sum = latestMessages.Where(x => x.SentAt.Date == hour.Date && x.SentAt.Hour == hour.Hour).Count();
                var item = new TimeStatisticItem(TimeRange.Create(hour, hour.AddHours(1).AddSeconds(-1)), sum);
                result.Add(item);
            });
            return result;
        }

        public IEnumerable<TimeStatisticItem> GetStatisticsPerMinute(IEnumerable<Message> latestMessages, TimeRange expectedTimeRange)
        {
            var result = new List<TimeStatisticItem>();
            expectedTimeRange.ForeachMinute((i, minute) =>
            {
                var sum = latestMessages.Where(x => x.SentAt.Date == minute.Date && x.SentAt.Hour == minute.Hour && x.SentAt.Minute == minute.Minute).Count();
                var item = new TimeStatisticItem(TimeRange.Create(minute, minute.AddMinutes(1).AddSeconds(-1)), sum);
                result.Add(item);
            });
            return result;
        }

        private IEnumerable<TimeStatisticItem> SumItems(IEnumerable<TimeStatisticItem> statisticsPerDay, int daysPerItem, TimeRange expectedTimeRange)
        {
            var result = new List<TimeStatisticItem>();
            for (var i = 0; i <= expectedTimeRange.DaysBetween / daysPerItem; i++)
            {
                var itemsToSum = statisticsPerDay.Skip(i * daysPerItem).Take(daysPerItem);
                var sum = itemsToSum.Sum(x => x.Value);
                var item = new TimeStatisticItem(TimeRange.Create(itemsToSum.First().Time.Start, itemsToSum.Last().Time.End), sum);
                result.Add(item);
            }
            return result;
        }

        private IEnumerable<Message> FilterMessages(IEnumerable<Message> latestMessages, IEnumerable<ServerDayStatistic> serverDayStatistics)
        {
            var latestPreGeneratedDate = serverDayStatistics.OrderBy(x => x.UpdatedAt).First();
            return latestMessages.Where(x =>
            {
                if (x.SentAt.Date > latestPreGeneratedDate.Date) //in newer day
                {
                    return true;
                }
                if (x.SentAt > latestPreGeneratedDate.CreatedAt) //in same day, but later
                {
                    return true;
                }
                return false;
            }).ToList();
        }
    }
}
