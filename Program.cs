using Akka.Actor;
using AkkaPersistenceSample.ActorModel.Actors;

namespace AkkaPersistenceSample
{
    internal class Program
    {
        private static ActorSystem _actorSystem { get; set; }
        private static IActorRef _strategyManager { get; set; }

        static void Main(string[] args)
        {
            _actorSystem = ActorSystem.Create("strategy-system");
            _strategyManager = _actorSystem.ActorOf<StrategyManagerActor>("StrategyManager");


            while (true)
            {
                DisplayInstructions();

                Console.WriteLine();
                Console.Write("Next command:");
                Console.ForegroundColor = ConsoleColor.Red;
                
                var action = Console.ReadLine()!;
                if (action.Contains("create"))
                {
                    CreateStrategy(action);
                }
                else if (action.Contains("change-price"))
                {
                    ChangePrice(action);
                }
                else if (action.Contains("execute-ammount"))
                {
                    ExecuteAmmountReceived(action);
                }
                else if (action.Contains("display-all"))
                {
                    DisplayAll(action);
                }
                else if (action.Contains("display"))
                {
                    Display(action);
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }

        private static void Display(string action)
        {
            var splitData = action.Split(' ')[2];
            var splitStrategyData = splitData.Split(',');
            
            //expected command
            //"strategy display {strategyId}
            var displayStrategyCommand = new ActorModel.Commands.DisplayStrategy()
            {
                StrategyId = Guid.Parse(splitStrategyData[0]),
            };

            _actorSystem
                .ActorSelection($"/user/StrategyManager/{displayStrategyCommand.StrategyId}")
                .Tell(displayStrategyCommand);
        }



        private static void DisplayAll(string action)
        {
            //expected command
            //"strategy display-all 
            var displayAllCommand = new ActorModel.Commands.DisplayAll();

            _actorSystem
                .ActorSelection($"/user/StrategyManager/*")
                .Tell(displayAllCommand);
        }

        private static void CreateStrategy(string action)
        {
            var splitData = action.Split(' ')[2];
            var splitStrategyData = splitData.Split(',');

            //expected command
            //"strategy create {userCode},{cyptoCurrency},{Side(buy or sell)},{EntryPrice},{GainPrice},{StopPrice},{Ammount}
            var createStrategyCommand = new ActorModel.Commands.CreateStrategy()
            {
                UserCode = splitStrategyData[0],
                CryptoCurrency = splitStrategyData[1],
                Side = splitStrategyData[2].ToUpper() == "BUY" ? ActorModel.Side.Buy : ActorModel.Side.Sell,
                EntryPrice = double.Parse(splitStrategyData[3]),
                GainPrice = double.Parse(splitStrategyData[4]),
                StopPrice = double.Parse(splitStrategyData[5]),
                Ammount = double.Parse(splitStrategyData[6]),
            };

            _strategyManager.Tell(createStrategyCommand);
        }

        private static void ExecuteAmmountReceived(string action)
        {
            
            var splitData = action.Split(' ')[2];
            var splitStrategyData = splitData.Split(',');

            //expected command
            //"strategy execute-ammount {strategyId},{ammount}
            var changePriceCommand = new ActorModel.Commands.ChangeExecutedAmmount()
            {
                StrategyId = Guid.Parse(splitStrategyData[0]),
                ExecutedAmmount = double.Parse(splitStrategyData[1]),
            };


            _actorSystem
                .ActorSelection($"/user/StrategyManager/*")
                .Tell(changePriceCommand);
        }

        private static void ChangePrice(string action)
        {
            var splitData = action.Split(' ')[2];
            var splitStrategyData = splitData.Split(',');

            //expected command
            //"strategy change-price {cryptoCurrency},{currentPrice}
            var changePriceCommand = new ActorModel.Commands.ChangePrice()
            {
                CryptoCurrency = splitStrategyData[0],
                CurrentPrice = double.Parse(splitStrategyData[1]),
            };

            _actorSystem
                .ActorSelection($"/user/StrategyManager/*")
                .Tell(changePriceCommand);
        }

        private static void DisplayInstructions()
        {
            Thread.Sleep(2000); // ensure console color set back to white

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("strategy create {userCode},{cyptoCurrency},{Side(buy or sell)},{EntryPrice},{GainPrice},{StopPrice},{Ammount}");
            Console.WriteLine("strategy change-price {cryptoCurrency},{currentPrice}");
            Console.WriteLine("strategy execute-ammount {strategyId},{ammount}");
            Console.WriteLine("strategy display {strategyId}");
            Console.WriteLine("strategy display-all");
        }
    }
}