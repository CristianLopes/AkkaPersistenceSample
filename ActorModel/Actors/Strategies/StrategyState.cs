namespace AkkaPersistenceSample.ActorModel.Actors.Strategies
{
    public class StrategyState
    {
        public string? UserCode { get; set; }
        public Guid StrategyId { get; set; }
        public string? CryptoCurrency { get; set; }
        public Side Side { get; set; }
        public double EntryPrice { get; set; }
        public double GainPrice { get; set; }
        public double StopPrice { get; set; }
        public double Ammount { get; set; }
        public double ExecutedAmmount { get; set; }
        public double PercentageExecuted { get; set; }
        public bool IsFinished { get; set; }
        public int EventCount { get; set; }
    }
}