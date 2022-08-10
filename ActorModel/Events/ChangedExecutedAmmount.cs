namespace AkkaPersistenceSample.ActorModel.Events
{
    internal class ChangedExecutedAmmount
    {
        public Guid StrategyId { get; set; }
        public double ExecutedAmmount { get; set; }
        public double Ammount { get; set; }
    }
}
