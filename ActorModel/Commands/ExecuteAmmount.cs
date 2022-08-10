namespace AkkaPersistenceSample.ActorModel.Commands
{
    internal class ChangeExecutedAmmount
    {
        public Guid StrategyId { get; set; }
        public double ExecutedAmmount { get; set; }
    }
}
