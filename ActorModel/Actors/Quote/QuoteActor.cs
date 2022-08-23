using Akka.Actor;
using AkkaPersistenceSample.ActorModel.Commands.Quote;

namespace AkkaPersistenceSample.ActorModel.Actors.Strategies.Quote
{
    internal class QuoteActor : ReceiveActor
    {
        public const string QuoteName = "quote";

        // HashSet automatically eliminates duplicates
        private readonly Dictionary<(IActorRef Actor, string CryptoCurrency), IActorRef> _subscribers;
        public IReadOnlyDictionary<(IActorRef Actor, string CryptoCurrency), IActorRef> Subscribers => _subscribers;

        public QuoteActor()
        {
            _subscribers = new Dictionary<(IActorRef, string), IActorRef>();

            Receive<SubscribeQuote>(sub =>
            {
                if (!_subscribers.ContainsKey((sub.Subscriber, sub.CryptoCurrency)))
                {
                    _subscribers.Add((sub.Subscriber, sub.CryptoCurrency), sub.Subscriber);
                }
            });

            Receive<UnsubscribeQuote>(sub =>
            {
                if (_subscribers.ContainsKey((sub.Subscriber, sub.CryptoCurrency)))
                {
                    _subscribers.Remove((sub.Subscriber, sub.CryptoCurrency));
                }
            });

            Context.ActorOf(Props.Create(() => new QuoteServiceActor(this)), name: QuoteServiceActor.QuoteServiceName);
        }
    }
}
