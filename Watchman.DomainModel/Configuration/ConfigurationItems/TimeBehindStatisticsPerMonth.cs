﻿using System;

namespace Watchman.DomainModel.Configuration.ConfigurationItems
{
    public class TimeBehindStatisticsPerMonth : MappedConfiguration<TimeSpan>
    {
        public override TimeSpan Value { get; set; } = TimeSpan.FromDays(365);

        public TimeBehindStatisticsPerMonth(ulong serverId) : base(serverId)
        {
        }
    }
}
