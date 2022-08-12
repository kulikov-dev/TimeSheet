using TimeSheet.Application.Interfaces;

namespace TimeSheet.Application.Logger
{
    /// <summary>
    /// Logger
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// List of wrappers for logging
        /// </summary>
        private static List<ILogWrapper> logWrappers = new List<ILogWrapper>();

        /// <summary>
        /// Attach a new wrapper
        /// </summary>
        /// <param name="logWrapper"> Wrapper </param>
        public static void Attach(ILogWrapper logWrapper)
        {
            logWrappers.Add(logWrapper);
        }

        /// <summary>
        /// Echo information message
        /// </summary>
        /// <param name="message"> Message </param>
        public static void Info(string message)
        {
            foreach (var logger in logWrappers)
            {
                logger.Info(message);
            }
        }

        /// <summary>
        /// Echo warning message
        /// </summary>
        /// <param name="message"> Message </param>
        public static void Warning(string message)
        {
            foreach (var logger in logWrappers)
            {
                logger.Warning(message);
            }
        }

        /// <summary>
        /// Echo error message
        /// </summary>
        /// <param name="message"> Message </param>
        /// <param name="ex"> Exception </param>
        public static void Error(string message, Exception? ex = null)
        {
            foreach (var logger in logWrappers)
            {
                logger.Error(message, ex);
            }
        }
    }
}
