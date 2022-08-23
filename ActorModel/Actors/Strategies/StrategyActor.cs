using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using AkkaPersistenceSample.ActorModel.Actors.Strategies.Quote;
using AkkaPersistenceSample.ActorModel.Commands;
using AkkaPersistenceSample.ActorModel.Commands.Quote;
using AkkaPersistenceSample.ActorModel.Events;
using System.Text.Json;

namespace AkkaPersistenceSample.ActorModel.Actors.Strategies
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
                DisplayHelper.WriteLine("DisplayAll command received", _loggingAdapter);
                DisplayHelper.PrintState(_state);
            });

            Command<ChangePrice>(
                command => !StrategyIsFinished() && IsSameCryptoCurrency(command),
                command => ReceivedChangePrice(command.CryptoCurrency, command.CurrentPrice)
                );

            Command<QuoteUpdate>(
                command => !StrategyIsFinished(),
                command => ReceivedChangePrice(command.CryptoCurrency, command.CurrentPrice)
                );

            Command<ChangeExecutedAmmount>(
                command => IsSameStrategyId(command),
                command => ReceivedExecuteAmmount(command));

            Command<FinishStrategyWithGain>(command =>
            {
                DisplayHelper.WriteLine($"StrategyFinishedWithGain task {DateTime.Now}", _loggingAdapter);
                Context.System
                  .ActorSelection($"/user/StrategyManager/{QuoteActor.QuoteName}")
                  .Tell(new UnsubscribeQuote(Self, _state.CryptoCurrency));
            });

            Command<FinishStrategyWithLoss>(command =>
            {
                DisplayHelper.WriteLine($"StrategyFinishedWithLoss task {DateTime.Now}", _loggingAdapter);

                Context.System
                  .ActorSelection($"/user/StrategyManager/{QuoteActor.QuoteName}")
                  .Tell(new UnsubscribeQuote(Self, _state.CryptoCurrency));
            });

            Recover<ChangedExecutedAmmount>(changedExecutedAmmountEvent =>
            {
                DisplayHelper.WriteLine($"Replaying ChangedExecutedAmmount event from journal", _loggingAdapter);

                _state.ExecutedAmmount += changedExecutedAmmountEvent.ExecutedAmmount;
                _state.EventCount++;
            });

            Recover<SnapshotOffer>(offer =>
            {
                DisplayHelper.WriteLine($"Received SnapshotOffer from snapshot offer", _loggingAdapter);
                _state = (StrategyState)offer.Snapshot;
            });

            Context.System
               .ActorSelection($"/user/StrategyManager/{QuoteActor.QuoteName}")
               .Tell(new SubscribeQuote(Self, _state.CryptoCurrency));
        }

        protected override bool Receive(object message)
        {
            return base.Receive(message);
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

        private void ReceivedChangePrice(string cryptoCurrency, double currentPrice)
        {
            DisplayHelper.WriteLine(
                $"Received PriceUpdate " +
                $"State CryptoCurrency: {_state.CryptoCurrency}" +
                $"Command CryptoCurrency: {cryptoCurrency}, " +
                $"Command CryptoCurrency: {currentPrice}", _loggingAdapter);


            if (_state.Side == Side.Buy)
            {
                if (currentPrice >= _state.GainPrice)
                {
                    FinishStrategyWithGain(_state);
                }
                else if (currentPrice <= _state.StopPrice)
                {
                    FinishStrategyWithLoss(_state);
                }
            }
            else
            {
                if (currentPrice <= _state.GainPrice)
                {
                    FinishStrategyWithGain(_state);
                }
                else if (currentPrice >= _state.StopPrice)
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
                var changeExecutedAmmount = new ChangeExecutedAmmount()
                {
                    StrategyId = _state.StrategyId,
                    ExecutedAmmount = _state.Ammount - _state.ExecutedAmmount,
                };

                Self.Tell(changeExecutedAmmount);
                Self.Tell(new FinishStrategyWithGain(strategyFinishedWithGain.StrategyId));
            });
        }

        private void FinishStrategyWithLoss(StrategyState state)
        {
            var @event = new StrategyFinishedWithLoss(state.StrategyId);
            Persist(@event, strategyFinishedWithLoss =>
            {
                var changeExecutedAmmount = new ChangeExecutedAmmount()
                {
                    StrategyId = _state.StrategyId,
                    ExecutedAmmount = _state.Ammount - _state.ExecutedAmmount,
                };

                Self.Tell(changeExecutedAmmount);
                Self.Tell(new StrategyFinishedWithLoss(strategyFinishedWithLoss.StrategyId));
            });
        }
    }
}