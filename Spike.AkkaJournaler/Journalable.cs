using System;
using Akka.Actor;

namespace Spike.AkkaJournaler
{
    public interface IJournalable
    {
        object Target { get; }

        DateTimeOffset Timestamp { set; }

        int Index { set; }
    }

    public class Journalable : IJournalable
    {
        public Journalable(object target, IActorRef sender)
        {
            Target = target;

            Sender = sender;
        }

        public IActorRef Sender { get; }

        public object Target { get; }

        public DateTimeOffset Timestamp
        {
            set { Sender.Tell(value); }
        }

        public int Index
        {
            set
            {
                /* not used currently */
            }
        }
    }
}