namespace AkkaPersistenceSample.ActorModel.Events
{
    internal class StrategyFinishedWithGain
    {
        public Guid StrategyId { get; set; }

        public StrategyFinishedWithGain(Guid strategyId)
        {
            StrategyId = strategyId;
        }
    }
}
