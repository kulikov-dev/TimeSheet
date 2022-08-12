// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using TimeSheet.Application;
using TimeSheet.Application.Logger;
using TimeSheet.Domain;

Log.Attach(new ConsoleWrapper());
Log.Attach(new NLogWrapper());

Console.WriteLine($"Введите '{Commands.Help}' для справки. Введите '{Commands.Report} {{месяц.год}}' чтобы создать табель учета рабочего времени.");
while (true)
{
    var command = Console.ReadLine();
    if (command == null)
    {
        continue;
    }

    switch (command.ToLower())
    {
        case Commands.Help:
            Console.WriteLine("Список доступных команд:");
            Console.WriteLine(" * report {месяц.год}: создать табель учета рабочего времени;");
            Console.WriteLine(" * path: указать путь к файлу со списком сотрудников;");
            Console.WriteLine(" * workers: открыть файл со списком сотрудников.");
            break;
        case Commands.Report:
            var commands = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var date = DateTime.Now;
            if (commands.Length == 2)
            {
                DateTime.TryParse(commands[1], out date);
            }

            Settings.ValidateWorkersFile();
            await ReportCreator.Create(date);
            Environment.Exit(0);
            break;
        case Commands.EditWorkersFilePath:
            Console.WriteLine("Пожалуйста, укажите полный путь до файла со списком сотрудников (*.json)");
            var path = Console.ReadLine();
            if (!File.Exists(path) || !Path.GetExtension(path).Equals("json"))
            {
                Console.WriteLine("Указан некорректный файл со списком сотрудников. Попробуйте еще раз.");
            }
            break;
        case Commands.OpenWorkersFile:
            Settings.ValidateWorkersFile();

            Process.Start(Settings.DefaultWorkersPath);
            break;
        default:
            Console.WriteLine($"Команда не найдена. Введите '{Commands.Help}' для справки.");
            break;
    }
}