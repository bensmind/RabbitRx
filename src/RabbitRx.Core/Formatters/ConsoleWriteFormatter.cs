using System;

namespace RabbitRx.Core.Formatters
{
    public class ConsoleWriteFormatter
    {
        public static void WriteLine(string message, ConsoleColor backGround = ConsoleColor.Black, ConsoleColor foreGround = ConsoleColor.White)
        {
            var bColor = Console.BackgroundColor;
            var fColor = Console.ForegroundColor;
            Console.BackgroundColor = backGround;
            Console.ForegroundColor = foreGround;

            Console.WriteLine(message);

            Console.BackgroundColor = bColor;
            Console.ForegroundColor = fColor;
        }
    }
}