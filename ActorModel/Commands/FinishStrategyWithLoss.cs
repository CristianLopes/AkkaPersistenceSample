namespace AkkaPersistenceSample.ActorModel.Commands
{
    internal class FinishStrategyWithLoss
    {
        public Guid StrategyId { get; set; }

        public FinishStrategyWithLoss(Guid strategyId)
        {
            StrategyId = strategyId;
        }
    }
}
