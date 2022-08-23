namespace AkkaPersistenceSample.ActorModel.Commands.Quote
{
    internal class QuoteUpdate
    {
        public QuoteUpdate(string cryptoCurrency, double currentPrice)
        {
            CryptoCurrency = cryptoCurrency;
            CurrentPrice = currentPrice;
        }

        public string CryptoCurrency { get; set; }
        public double CurrentPrice { get; set; }
    }
}
