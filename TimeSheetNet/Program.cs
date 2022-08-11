// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using TimeSheetNet.Application;

Console.WriteLine("Введите '?' для справки. Введите 'report {месяц.год}' чтобы создать табель учета рабочего времени.");
while (true)
{
    var command = Console.ReadLine();
    if (command == null)
    {
        continue;
    }

    switch (command.ToLower())
    {
        case "?":
            Console.WriteLine("Список доступных команд:");
            Console.WriteLine(" * report {месяц.год}: создать табель учета рабочего времени;");
            Console.WriteLine(" * path: указать путь к файлу со списком сотрудников;");
            Console.WriteLine(" * workers: открыть файл со списком сотрудников.");
            break;
        case "report":
            var commands = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var date = DateTime.Now;
            if (commands.Length == 2)
            {
                DateTime.TryParse(commands[1], out date);
            }
            ValidateWorkersFile();
            await ReportCreator.Create(date);
            Environment.Exit(0);
            break;
        case "path":
            Console.WriteLine("Пожалуйста, укажите полный путь до файла со списком сотрудников (*.json)");
            var path = Console.ReadLine();
            if (!File.Exists(path) || !Path.GetExtension(path).Equals("json"))
            {
                Console.WriteLine("Указан некорректный файл со списком сотрудников. Попробуйте еще раз.");
            }
            break;
        case "workers":
            ValidateWorkersFile();

            Process.Start(Settings.DefaultWorkersPath);
            break;
        default:
            Console.WriteLine("Команда не найдена. Введите ? для справки.");
            break;
    }
}

void ValidateWorkersFile()
{
    if (!File.Exists(Settings.WorkersPath))
    {
        if (!Settings.WorkersPath.Equals(Settings.DefaultWorkersPath))
        {
            Console.WriteLine("Предупреждение. Указанный файл со списком сотрудников не найден. Будет использован файл по умолачнию");
        }

        if (!File.Exists(Settings.DefaultWorkersPath))
        {
            File.Create(Settings.DefaultWorkersPath);
        }

        Settings.WorkersPath = Settings.DefaultWorkersPath; //TODO save
    }
}