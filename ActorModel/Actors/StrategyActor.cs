using Akka.Persistence;
using AkkaPersistenceSample.ActorModel.Commands;
using AkkaPersistenceSample.ActorModel.Events;
using System.Text.Json;

namespace AkkaPersistenceSample.ActorModel.Actors
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

    public class StrategyActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"strategy-{_state.StrategyId}";
        private StrategyState _state;

        public StrategyActor(string userCode, Guid strategyId, string cryptoCurrency, Side side, double entryPrice, double gainPrice, double stopPrice, double ammount)
        {
            _state = new StrategyState
            {
                UserCode = userCode,
                StrategyId = strategyId,
                CryptoCurrency = cryptoCurrency,
                Side = side,
                EntryPrice = entryPrice,
                GainPrice = gainPrice,
                StopPrice = stopPrice,
                Ammount = ammount,
                ExecutedAmmount = 0
            };

            Command<DisplayStrategy>(command =>
            {
                DisplayHelper.WriteLine($"DisplayStrategy command received {command.StrategyId}", ConsoleColor.Blue);
                DisplayHelper.WriteLine(JsonSerializer.Serialize(_state), ConsoleColor.Blue);
            });

            Command<DisplayAll>(command =>
            {
                DisplayHelper.WriteLine("DisplayAll command received", ConsoleColor.Blue);
                DisplayHelper.WriteLine(JsonSerializer.Serialize(_state), ConsoleColor.Blue);
            });


            Command<ChangePrice>(
                command => !StrategyIsFinished() && IsSameCryptoCurrency(command),
                (command => ReceivedChangePrice(command))
                );



            Command<ChangeExecutedAmmount>(
                command => IsSameStrategyId(command),
                (command => ReceivedExecuteAmmount(command)));

            Recover<ChangedExecutedAmmount>(changedExecutedAmmountEvent =>
            {
                DisplayHelper.WriteLine($"{_state.StrategyId} replaying ChangedExecutedAmmount event from journal");
                
                _state.ExecutedAmmount += changedExecutedAmmountEvent.ExecutedAmmount;
                _state.EventCount++;
            });

            Recover<SnapshotOffer>(offer =>
            {

                DisplayHelper.WriteLine($"{_state.StrategyId} received SnapshotOffer from snapshot offer");
                _state = (StrategyState)offer.Snapshot;
            });
        }

        private bool StrategyIsFinished() => _state.IsFinished;

        private bool IsSameCryptoCurrency(ChangePrice command) => command.CryptoCurrency == _state.CryptoCurrency;

        private bool IsSameStrategyId(ChangeExecutedAmmount command) => command.StrategyId == _state.StrategyId;

        private void ReceivedExecuteAmmount(ChangeExecutedAmmount command)
        {
            var @event = new ChangedExecutedAmmount()
            {
                StrategyId = command.StrategyId,
                ExecutedAmmount = command.ExecutedAmmount,
                Ammount = _state.Ammount,
            };

            Persist(@event, changedExecutedAmmount =>
            {
                _state.ExecutedAmmount += changedExecutedAmmount.ExecutedAmmount;
                _state.PercentageExecuted = 100 * _state.ExecutedAmmount / _state.Ammount;
                _state.IsFinished = _state.ExecutedAmmount == _state.Ammount;

                DisplayHelper.WriteLine($"Strategy ExecutedAmmount event for {changedExecutedAmmount.StrategyId} " +
                    $"New State {System.Text.Json.JsonSerializer.Serialize(_state)}");

                _state.EventCount++;

                if (_state.EventCount == 5)
                {
                    DisplayHelper.WriteLine($"{_state.StrategyId} saving snapshot");

                    _state.EventCount = 0;

                    SaveSnapshot(_state);

                    DisplayHelper.WriteLine($"{_state.StrategyId} resetting event count to 0");
                }
            });
        }

        private void ReceivedChangePrice(ChangePrice command)
        {
            DisplayHelper.WriteLine(
                $"Received PriceUpdate " +
                $"State CryptoCurrency: {_state.CryptoCurrency}" +
                $"Command CryptoCurrency: {command.CryptoCurrency}, " +
                $"Command CryptoCurrency: {command.CurrentPrice}");

            if (_state.Side == Side.Buy)
            {
                if (command.CurrentPrice >= _state.GainPrice)
                {
                    FinishStrategyWithGain(_state);
                }
                else if (command.CurrentPrice <= _state.StopPrice)
                {
                    FinishStrategyWithLoss(_state);
                }
            }
            else
            {
                if (command.CurrentPrice <= _state.GainPrice)
                {
                    FinishStrategyWithGain(_state);
                }
                else if (command.CurrentPrice >= _state.StopPrice)
                {
                    FinishStrategyWithLoss(_state);
                }
            }
        }
        private void FinishStrategyWithGain(StrategyState state)
        {
            var @event = new StrategyFinishedWithGain(state.StrategyId);
            Persist(@event, strategyFinishedWithGain =>
            {
                DisplayHelper.WriteLine($"Strategy finished with gain {strategyFinishedWithGain.StrategyId}");

                var changeExecutedAmmount = new ChangeExecutedAmmount()
                {
                    StrategyId = _state.StrategyId,
                    ExecutedAmmount = _state.Ammount - _state.ExecutedAmmount,
                };

                Self.Tell(changeExecutedAmmount, Self);
                Self.Tell(new FinishStrategyWithGain(strategyFinishedWithGain.StrategyId), Self);
            });
        }

        private void FinishStrategyWithLoss(StrategyState state)
        {
            
            var @event = new StrategyFinishedWithLoss(state.StrategyId);
            Persist(@event, strategyFinishedWithLoss =>
            {
                DisplayHelper.WriteLine($"Strategy finished with loss {strategyFinishedWithLoss.StrategyId}");
                
                var changeExecutedAmmount = new ChangeExecutedAmmount()
                {
                    StrategyId = _state.StrategyId,
                    ExecutedAmmount = _state.Ammount - _state.ExecutedAmmount,
                };

                Self.Tell(changeExecutedAmmount, Self);
                Self.Tell(new StrategyFinishedWithLoss(strategyFinishedWithLoss.StrategyId), Self);
            });
        }
    }
}