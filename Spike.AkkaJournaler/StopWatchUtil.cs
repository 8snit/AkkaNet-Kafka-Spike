using System;
using System.Diagnostics;

namespace Spike.AkkaJournaler
{
    public static class StopWatchUtil
    {
        public static TimeSpan Measure(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}