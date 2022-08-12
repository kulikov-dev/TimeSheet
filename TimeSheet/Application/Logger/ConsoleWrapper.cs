using TimeSheet.Application.Interfaces;

namespace TimeSheet.Application.Logger
{
    /// <summary>
    /// Log wrapper to output in a Console
    /// </summary>
    internal sealed class ConsoleWrapper : ILogWrapper
    {
        /// <summary>
        /// Echo information message
        /// </summary>
        /// <param name="message"> Message </param>
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Echo warning message
        /// </summary>
        /// <param name="message"> Message </param>
        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Предупреждение. {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Echo error message
        /// </summary>
        /// <param name="message"> Message </param>
        /// <param name="ex"> Exception </param>
        public void Error(string message, Exception? ex = null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Ошибка. {message}");
            Console.ResetColor();
        }
    }
}