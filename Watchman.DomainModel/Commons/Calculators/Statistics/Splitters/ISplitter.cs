﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watchman.Common.Models;
using Watchman.Integrations.MongoDB;

namespace Watchman.DomainModel.Commons.Calculators.Statistics.Splitters
{
    public interface ISplitter
    {
        public IEnumerable<KeyValuePair<TimeRange, IEnumerable<T>>> Split<T>(IEnumerable<T> collection) where T : Entity;
    }
}
