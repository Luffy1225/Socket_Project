using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luffy_Tool
{

    public enum ConsoleColorType
    {
        Default,
        Reply,
        Send,
        Warning,
        Error,
        Notice,
        Announce
    }

    public class Print_Tool
    {
        public static void WriteLine(string message, ConsoleColorType ColorType)
        {
            var colorMapping = new Dictionary<ConsoleColorType, ConsoleColor>
        {
            { ConsoleColorType.Default, ConsoleColor.White },
            { ConsoleColorType.Reply, ConsoleColor.Cyan },
            { ConsoleColorType.Send, ConsoleColor.Green },
            { ConsoleColorType.Warning, ConsoleColor.Yellow },
            { ConsoleColorType.Error, ConsoleColor.Red },
            { ConsoleColorType.Notice, ConsoleColor.Magenta },
            { ConsoleColorType.Announce, ConsoleColor.Blue }
        };


            // 儲存目前的控制台顏色
            ConsoleColor originalColor = Console.ForegroundColor;

            ConsoleColor color;
            // 設定控制台顏色為指定顏色
            if (colorMapping.TryGetValue(ColorType, out color))
            {
                Console.ForegroundColor = color;
            }
            Console.WriteLine(message);

            // 恢復控制台顏色
            Console.ForegroundColor = originalColor;
        }
        public static void WriteLine(string message, ConsoleColor Color)
        {

            // 儲存目前的控制台顏色
            ConsoleColor originalColor = Console.ForegroundColor;

            // 設定控制台顏色為指定顏色

            Console.ForegroundColor = Color;

            Console.WriteLine(message);

            // 恢復控制台顏色
            Console.ForegroundColor = originalColor;
        }
        
    }
}
