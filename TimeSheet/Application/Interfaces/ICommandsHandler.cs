using TimeSheet.Application.Structs;

namespace TimeSheet.Application.Interfaces
{
    /// <summary>
    /// Interface for console commands handler
    /// </summary>
    internal interface ICommandsHandler
    {
        /// <summary>
        /// Get console commands
        /// </summary>
        /// <returns> List of console commands </returns>
        List<ConsoleCommand> GetCommands();

        /// <summary>
        /// Process command
        /// </summary>
        /// <param name="command"> A command </param>
        /// <returns> A command processed </returns>
        Task<bool> Process(string command);
    }
}