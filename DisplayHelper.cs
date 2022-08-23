using Akka.Event;
using AkkaPersistenceSample.ActorModel.Actors.Strategies;

namespace AkkaPersistenceSample
{
    static class DisplayHelper
    {
        public static void WriteLine(string message, ILoggingAdapter loggingAdapter)
        {
            //var originalColor = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine(message);
            loggingAdapter.Info(message);
            //Console.ForegroundColor = originalColor;
        }

        internal static void PrintState(StrategyState state)
        {
            Console.WriteLine($"" +
                $"\nStrategyId: {state.StrategyId} - {(state.IsFinished ? "FINISHED" : "EXECUTING")}" +
                $"\nCryptoCurrency: {state.CryptoCurrency}" +
                $"\nSide: {state.Side}" +
                $"\nEntryPrice: {state.EntryPrice}" +
                $"\nGainPrice: {state.GainPrice}" +
                $"\nStopPrice: {state.StopPrice}" +
                $"\nExecutedAmmount/Ammount: {state.ExecutedAmmount}/{state.Ammount}" +
                $"");
        }
    }
}