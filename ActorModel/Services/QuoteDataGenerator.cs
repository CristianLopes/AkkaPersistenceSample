using AkkaPersistenceSample.ActorModel.Commands.Quote;

namespace AkkaPersistenceSample.ActorModel.Services
{
    internal class QuoteDataGenerator
    {
        private record QuoteFakeInfo(string Crypto, double minValue, double maxvalue);
        private static IList<QuoteFakeInfo> cryptos = new List<QuoteFakeInfo>
        {
            new QuoteFakeInfo("BTC", 15000, 30000),
            new QuoteFakeInfo("ETH", 750, 4000),
            new QuoteFakeInfo("SOL", 20, 250),
            new QuoteFakeInfo("AXS", 3, 150),
            new QuoteFakeInfo("ADA", 0.5, 5),
            new QuoteFakeInfo("DOT", 1, 25),
        };


        public static QuoteUpdate GenerateQuoteUpdate()
        {
            Random rnd = new Random();
            int index = rnd.Next(cryptos.Count);
            var item = cryptos[index];
            var newPrice = rnd.NextDouble() * (item.maxvalue - item.minValue) + item.minValue;
            return new QuoteUpdate(item.Crypto, newPrice);
        }
    }
}
