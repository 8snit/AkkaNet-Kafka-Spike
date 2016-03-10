using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;

namespace Spike.AkkaJournaler
{
    public sealed class Statistic
    {
        public static Statistic Instance = new Statistic();
    }

    public class JournalActor : ReceiveActor
    {
        private readonly List<Journalable> _bufferedJournables = new List<Journalable>();
        private readonly IJournalPersistor _journalPersistor;

        public JournalActor(IJournalPersistor journalPersistor, long batchDelayMs = 10L, int batchSize = 100)
        {
            _journalPersistor = journalPersistor;
            Receive<ReceiveTimeout>(_ => Persist());
            Receive<Statistic>(_ => Sender.Tell(Statistic));
            Receive<object>(target =>
            {
                var journalable = new Journalable(target, Sender);
                _bufferedJournables.Add(journalable);
                if (_bufferedJournables.Count >= batchSize)
                {
                    Persist();
                }
            });

            SetReceiveTimeout(TimeSpan.FromMilliseconds(batchDelayMs));
        }

        public List<JournalStatisticEntry> Statistic { get; } = new List<JournalStatisticEntry>();

        private void Persist()
        {
            if (_bufferedJournables.Count <= 0)
                return;

            var journalables = _bufferedJournables.ToArray();
            _bufferedJournables.Clear();

            var journalStatisticEntry = new JournalStatisticEntry {BatchSize = journalables.Length};
            journalStatisticEntry.OverallElapsed = StopWatchUtil.Measure(() =>
            {
                journalStatisticEntry.WritingOnlyElapsed =
                    StopWatchUtil.Measure(
                        () =>
                        {
                            _journalPersistor.WriteAsync(DateTimeOffset.Now, journalables).ContinueWith(result =>
                            {
                                foreach (var journalable in journalables)
                                {
                                    if (result.IsCanceled || result.IsFaulted)
                                        journalable.Sender.Tell(new Failure {Exception = result.Exception}, Self);

                                    journalable.Sender.Tell(journalable);
                                }
                            }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                                .PipeTo(Self);
                        });
            });
            Statistic.Add(journalStatisticEntry);
        }
    }
}