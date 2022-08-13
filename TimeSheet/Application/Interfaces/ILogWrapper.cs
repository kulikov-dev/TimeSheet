namespace TimeSheet.Application.Interfaces
{
    /// <summary>
    /// Interface for log wrappers
    /// </summary>
    internal interface ILogWrapper
    {
        /// <summary>
        /// Echo information message
        /// </summary>
        /// <param name="message"> Message </param>
        void Info(string message);

        /// <summary>
        /// Echo warning message
        /// </summary>
        /// <param name="message"> Message </param>
        void Warning(string message);

        /// <summary>
        /// Echo error message
        /// </summary>
        /// <param name="message"> Message </param>
        /// <param name="ex"> Exception </param>
        void Error(string message, Exception? ex = null);
    }
}