namespace AkkaPersistenceSample.ActorModel.Commands
{
    internal class ChangePrice
    {
        public string CryptoCurrency { get; set; }
        public double CurrentPrice { get; set; }
    }
}
