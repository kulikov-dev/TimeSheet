using System.Diagnostics;
using System.Text.Json;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Application.Structs;
using TimeSheet.Data;

namespace TimeSheet.Domain.Providers
{
    internal sealed class DepartmentFileProvider : IDepartmentProvider
    {
        /// <summary>
        /// Load department information
        /// </summary>
        /// <returns> Department information </returns>
        public async Task<DepartmentInfo?> Load()
        {
            if (string.IsNullOrWhiteSpace(DepartmentFileProviderSettings.SourcePath))
            {
                Log.Error("Не удалось загрузить данные по сотрудникам. Проверьте целостность файла данных.");
                return null;
            }

            await using (var fileStream = new FileStream(DepartmentFileProviderSettings.SourcePath, FileMode.Open, FileAccess.Read))
            {
                var result = await JsonSerializer.DeserializeAsync<DepartmentInfo>(fileStream);
                if (result == null)
                {
                    Log.Error("Не удалось загрузить данные по сотрудникам. Проверьте корректность файла данных");
                }

                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks> Used by reflection. Do not remove </remarks>
        internal sealed class DepartmentFileProviderCommandHandler : ICommandsHandler
        {

            /// <summary>
            /// Open a file with workers
            /// </summary>
            private const string OpenWorkersFile = "workers";

            /// <summary>
            /// Setup path to a customer file with workers
            /// </summary>
            private const string ShowWorkersFilePath = "print_path";

            private const string EditfilePath = "path";

            /// <summary>
            /// Get console commands
            /// </summary>
            /// <returns> List of console commands </returns>
            public List<ConsoleCommand> GetCommands()
            {
                return new List<ConsoleCommand>()
                {
                    new (EditfilePath, $" * {EditfilePath}: указать путь к файлу со списком сотрудников;", 4),
                    new (ShowWorkersFilePath, $" * {ShowWorkersFilePath}: вывести путь к файлу со списком сотрудников;", 2),
                    new(OpenWorkersFile, $" * {OpenWorkersFile}: открыть файл со списком сотрудников.", 3)
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
                    if (localCommand.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase))
                    {
                        switch (localCommand.Name)
                        {
                            case OpenWorkersFile:
                                DepartmentFileProviderSettings.LaunchStorageFile();
                                break;
                            case ShowWorkersFilePath:
                                DepartmentFileProviderSettings.Print();
                                break;
                            case EditfilePath:
                                Console.WriteLine("Пожалуйста, укажите полный путь до файла со списком сотрудников (*.json)");
                                var path = Console.ReadLine();
                                DepartmentFileProviderSettings.UpdateWorkersFilePath(path);
                                break;
                        }
                        return Task.FromResult(true);
                    }
                }

                return Task.FromResult(false);
            }
        }

        internal static class DepartmentFileProviderSettings
        {
            /// <summary>
            /// 
            /// </summary>
            private static readonly string DefaultSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workers.json");
            private static string? _sourcePath = DefaultSourcePath;

            static DepartmentFileProviderSettings()
            {
                if (!string.IsNullOrWhiteSpace(Settings.Default.DepartmentStorageSourcePath))
                {
                    SourcePath = Settings.Default.DepartmentStorageSourcePath;
                    ValidateWorkersFile();
                }
            }

            internal static string? SourcePath
            {
                get => _sourcePath;

                private set
                {
                    if (_sourcePath == value)
                    {
                        return;
                    }

                    _sourcePath = value;
                    Settings.Default.DepartmentStorageSourcePath = _sourcePath;

                }
            }

            internal static void LaunchStorageFile()
            {
                ValidateWorkersFile();
                Process.Start(SourcePath);
            }

            private static bool ValidateWorkersFile()
            {
                if (!File.Exists(SourcePath))
                {
                    if (!SourcePath.Equals(DefaultSourcePath))
                    {
                        Log.Warning("Указанный файл со списком сотрудников не найден. Будет использован файл по умолачнию");
                    }

                    if (!File.Exists(DefaultSourcePath))
                    {
                        File.Create(DefaultSourcePath);
                    }

                    SourcePath = DefaultSourcePath;
                    return false;
                }

                return true;
            }

            internal static void UpdateWorkersFilePath(string? filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath) || filePath == SourcePath)
                {
                    return;
                }

                SourcePath = filePath;
                ValidateWorkersFile();
            }

            internal static void Print()
            {
                Console.WriteLine(SourcePath);
            }
        }
    }
}
