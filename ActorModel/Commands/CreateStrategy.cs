namespace AkkaPersistenceSample.ActorModel.Commands
{
    internal class CreateStrategy
    {
        public string UserCode { get; set; }
        public string CryptoCurrency { get; set; }
        public Side Side { get; set; }
        public double EntryPrice { get; set; }
        public double GainPrice { get; set; }
        public double StopPrice { get; set; }
        public double Ammount { get; set; }
    }
}
