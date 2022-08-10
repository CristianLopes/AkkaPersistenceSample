namespace AkkaPersistenceSample.ActorModel.Events
{
    internal class StrategyFinishedWithLoss
    {
        public Guid StrategyId { get; set; }

        public StrategyFinishedWithLoss(Guid strategyId)
        {
            StrategyId = strategyId;
        }
    }
}
