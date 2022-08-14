using Akka.Event;
using Akka.Persistence;
using AkkaPersistenceSample.ActorModel.Commands;
using AkkaPersistenceSample.ActorModel.Events;
using System.Text.Json;

namespace AkkaPersistenceSample.ActorModel.Actors
{
    public class StrategyActor : ReceivePersistentActor
    {
        private readonly ILoggingAdapter _loggingAdapter = Context.GetLogger();
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
                DisplayHelper.WriteLine($"DisplayStrategy command received {command.StrategyId}", loggingAdapter: _loggingAdapter);
                DisplayHelper.PrintState(_state);
            });

            Command<DisplayAll>(command =>
            {
                DisplayHelper.WriteLine("DisplayAll command received",_loggingAdapter);
                DisplayHelper.PrintState(_state);
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
                DisplayHelper.WriteLine($"{_state.StrategyId} replaying ChangedExecutedAmmount event from journal", _loggingAdapter);
                
                _state.ExecutedAmmount += changedExecutedAmmountEvent.ExecutedAmmount;
                _state.EventCount++;
            });

            Recover<SnapshotOffer>(offer =>
            {
                DisplayHelper.WriteLine($"{_state.StrategyId} received SnapshotOffer from snapshot offer", _loggingAdapter);
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

                DisplayHelper.WriteLine($"Strategy ExecutedAmmount event for {changedExecutedAmmount.StrategyId}", _loggingAdapter);

                _state.EventCount++;

                if (_state.EventCount == 5)
                {
                    _state.EventCount = 0;

                    SaveSnapshot(_state);

                    DisplayHelper.WriteLine($"{_state.StrategyId} snapshot saved", _loggingAdapter);
                }
            });
        }

        private void ReceivedChangePrice(ChangePrice command)
        {
            DisplayHelper.WriteLine(
                $"Received PriceUpdate " +
                $"State CryptoCurrency: {_state.CryptoCurrency}" +
                $"Command CryptoCurrency: {command.CryptoCurrency}, " +
                $"Command CryptoCurrency: {command.CurrentPrice}", _loggingAdapter);

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
                DisplayHelper.WriteLine($"Strategy finished with gain {strategyFinishedWithGain.StrategyId}", _loggingAdapter);

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
                DisplayHelper.WriteLine($"Strategy finished with loss {strategyFinishedWithLoss.StrategyId}", _loggingAdapter);
                
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