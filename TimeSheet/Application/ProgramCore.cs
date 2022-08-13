using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Structs;

namespace TimeSheet.Application
{
    /// <summary>
    /// Core
    /// </summary>
    internal static class ProgramCore
    {
        /// <summary>
        /// List of a console command handlers
        /// </summary>
        internal static List<ICommandsHandler> ConsoleCommandHandlers { get; } = new();

        /// <summary>
        /// Core initialization
        /// </summary>
        /// <remarks> MUST init at the program startup </remarks>
        internal static void Init()
        {
            ConsoleCommandHandlers.Clear();
            var handlerType = typeof(ICommandsHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type != handlerType && handlerType.IsAssignableFrom(type));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is ICommandsHandler instance)
                {
                    ConsoleCommandHandlers.Add(instance);
                }
            }
        }

        /// <summary>
        /// Core console commands handler
        /// </summary>
        /// <remarks> Used by reflection. Do not remove </remarks>
        internal sealed class ApplicationCommandHandler : ICommandsHandler
        {
            /// <summary>
            /// Show help information
            /// </summary>
            internal const string Help = "?";

            /// <summary>
            /// Exit application
            /// </summary>
            private const string Exit = "exit";

            /// <summary>
            /// Get console commands
            /// </summary>
            /// <returns> List of console commands </returns>
            public List<ConsoleCommand> GetCommands()
            {
                return new List<ConsoleCommand>()
                {
                    new (Help, $" * {Help}: показать справку;", 5),
                    new (Exit, $" * {Exit}: выйти из приложения;",6 )
                };
            }

            /// <summary>
            /// Process command
            /// </summary>
            /// <param name="command"> A command </param>
            /// <returns> A command processed </returns>
            public Task<bool> Process(string command)
            {
                var commands = GetCommands();
                foreach (var localCommand in commands)
                {
                    if (!localCommand.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    switch (localCommand.Name)
                    {
                        case Help:
                            Console.WriteLine("Список доступных команд:");
                            var allCommands = new List<ConsoleCommand>();
                            foreach (var handler in ConsoleCommandHandlers)
                            {
                                allCommands.AddRange(handler.GetCommands());
                            }

                            allCommands.Sort();
                            foreach (var commandInfo in allCommands)
                            {
                                Console.WriteLine(commandInfo.Help);
                            }

                            Console.WriteLine();
                            break;
                        case Exit:
                            Environment.Exit(0);
                            break;
                    }

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
    }
}