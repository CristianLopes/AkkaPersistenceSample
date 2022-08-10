namespace AkkaPersistenceSample
{
    static class DisplayHelper
    {
        public static void WriteLine(string message, ConsoleColor consoleColor = ConsoleColor.Green)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }
}