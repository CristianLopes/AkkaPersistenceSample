using Akka.Actor;

namespace AkkaPersistenceSample.ActorModel.Commands.Quote
{
    internal class UnsubscribeQuote
    {
        public UnsubscribeQuote(IActorRef subscriber, string cryptoCurrency)
        {
            Subscriber = subscriber;
            CryptoCurrency = cryptoCurrency;
        }

        public IActorRef Subscriber { get; private set; }
        public string CryptoCurrency { get; private set; }
    }
}
