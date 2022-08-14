using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using AkkaPersistenceSample.ActorModel.Commands;
using AkkaPersistenceSample.ActorModel.Events;

namespace AkkaPersistenceSample.ActorModel.Actors
{
    internal class StrategyManagerActor : ReceivePersistentActor
    {
        private readonly ILoggingAdapter _loggingAdapter =Context.GetLogger();

        public override string PersistenceId => "strategy-manager";

        public StrategyManagerActor()
        {
            Command<CreateStrategy>(command =>
            {
                var newId = Guid.NewGuid();
                DisplayHelper.WriteLine($"StrategyManagerActor received CreateStrategy command for {newId}", _loggingAdapter);
                var @event = new StrategyCreated()
                {
                    UserCode = command.UserCode,
                    StrategyId = newId,
                    CryptoCurrency = command.CryptoCurrency,
                    Side = command.Side,
                    EntryPrice = command.EntryPrice,
                    GainPrice = command.GainPrice,
                    StopPrice = command.StopPrice,
                    Ammount = command.Ammount
                };

                Persist(@event, strategyCreatedEvent =>
                {
                    DisplayHelper.WriteLine($"StrategyManagerActor StrategyCreated event persisted for {strategyCreatedEvent}", _loggingAdapter);

                    Context.ActorOf(
                        Props.Create(
                            () => 
                            new StrategyActor(
                                strategyCreatedEvent.UserCode,
                                strategyCreatedEvent.StrategyId,
                                strategyCreatedEvent.CryptoCurrency, 
                                strategyCreatedEvent.Side,
                                strategyCreatedEvent.EntryPrice,
                                strategyCreatedEvent.GainPrice,
                                strategyCreatedEvent.StopPrice,
                                strategyCreatedEvent.Ammount
                                )), name: strategyCreatedEvent.StrategyId.ToString());
                });
            });

            Recover<StrategyCreated>(playerCreatedEvent =>
            {
                DisplayHelper.WriteLine($"StrategyManagerActor replaying StrategyCreated event for {playerCreatedEvent}", _loggingAdapter);

                Context.ActorOf(
                    Props.Create(
                        () =>
                        new StrategyActor(
                            playerCreatedEvent.UserCode,
                            playerCreatedEvent.StrategyId,
                            playerCreatedEvent.CryptoCurrency,
                            playerCreatedEvent.Side,
                            playerCreatedEvent.EntryPrice,
                            playerCreatedEvent.GainPrice,
                            playerCreatedEvent.StopPrice,
                            playerCreatedEvent.Ammount
                            )), name: playerCreatedEvent.StrategyId.ToString());
            });
        }
    }
}
