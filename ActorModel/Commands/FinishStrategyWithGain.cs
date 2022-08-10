namespace AkkaPersistenceSample.ActorModel.Commands
{
    internal class FinishStrategyWithGain
    {
        public Guid StrategyId { get; set; }

        public FinishStrategyWithGain(Guid strategyId)
        {
            StrategyId = strategyId;
        }
    }
}
