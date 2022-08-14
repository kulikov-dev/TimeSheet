using TimeSheet.Application;
using TimeSheet.Application.Logger;

Log.Attach(new ConsoleWrapper());
Log.Attach(new NLogWrapper());

ProgramCore.Init();

Console.WriteLine($"Добро пожаловать в TimeSheet: программу для составления табелей учета рабочего времени. Введите '{ProgramCore.ApplicationCommandHandler.Help}' для справки.");
while (true)
{
    var command = Console.ReadLine()?.ToLower().Trim();
    if (string.IsNullOrWhiteSpace(command))
    {
        continue;
    }

    var isCommandProcessed = false;
    foreach (var handler in ProgramCore.ConsoleCommandHandlers)
    {
        if (await handler.Process(command))
        {
            isCommandProcessed = true;
            break;
        }
    }

    if (!isCommandProcessed)
    {
        Console.WriteLine($"Команда не найдена. Введите '{ProgramCore.ApplicationCommandHandler.Help}' для справки.");
        Console.WriteLine();
    }
}