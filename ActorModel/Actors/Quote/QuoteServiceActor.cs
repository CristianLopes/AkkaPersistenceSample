using Akka.Actor;
using AkkaPersistenceSample.ActorModel.Commands.Quote;
using AkkaPersistenceSample.ActorModel.Services;

namespace AkkaPersistenceSample.ActorModel.Actors.Strategies.Quote
{
    internal class QuoteServiceActor : ReceiveActor
    {
        public const string QuoteServiceName = "quote-service";
        public QuoteActor _quoteActor;
        public QuoteServiceActor(QuoteActor quoteActor)
        {
            _quoteActor = quoteActor;

            Receive<GetQuoteUpdate>(message =>
            {
                var quoteUpdate = QuoteDataGenerator.GenerateQuoteUpdate();
                // notify each subscriber
                foreach (var sub in _quoteActor.Subscribers)
                {
                    if (IsSameCryptoCurrency(quoteUpdate, sub))
                    {
                        sub.Value.Tell(quoteUpdate);
                    }
                }
            });
        }

        protected override void PreStart()
        {
            Context.System.Scheduler
                .ScheduleTellRepeatedly(
                 TimeSpan.FromMilliseconds(250),
                 TimeSpan.FromMilliseconds(250),
                 Self,
                 new GetQuoteUpdate(), 
                 Self);
        }

        private static bool IsSameCryptoCurrency(QuoteUpdate message, KeyValuePair<(IActorRef Actor, string CryptoCurrency), IActorRef> sub)
        {
            return message.CryptoCurrency == sub.Key.CryptoCurrency;
        }
    }
}
