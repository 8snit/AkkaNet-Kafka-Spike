using System;

namespace Spike.AkkaJournaler
{
    public struct JournalStatisticEntry
    {
        public int BatchSize { get; set; }

        public TimeSpan OverallElapsed { get; set; }

        public TimeSpan WritingOnlyElapsed { get; set; }
    }
}