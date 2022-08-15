using System.Text.Json;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Application.Structs;
using TimeSheet.Application.Utils;
using TimeSheet.Data;

namespace TimeSheet.Domain.Providers
{
    /// <summary>
    /// Department storage file data source
    /// </summary>
    internal sealed class DepartmentFileProvider : IDepartmentProvider
    {
        /// <summary>
        /// Load department information
        /// </summary>
        /// <returns> Department information </returns>
        public async Task<DepartmentInfo?> Load()
        {
            const string errorMessage = "Не удалось загрузить данные по сотрудникам. Проверьте целостность файла данных.";
            if (string.IsNullOrWhiteSpace(DepartmentFileProviderSettings.SourcePath))
            {
                Log.Error(errorMessage);
                return null;
            }

            await using var fileStream = new FileStream(DepartmentFileProviderSettings.SourcePath, FileMode.Open, FileAccess.Read);
            var result = await JsonSerializer.DeserializeAsync<DepartmentInfo>(fileStream);
            if (result == null)
            {
                Log.Error(errorMessage);
            }

            return result;
        }

        /// <summary>
        /// Class command handler
        /// </summary>
        /// <remarks> Used by reflection. Do not remove </remarks>
        internal sealed class DepartmentFileProviderCommandHandler : ICommandsHandler
        {
            /// <summary>
            /// Command to open a file in the Explorer
            /// </summary>
            private const string OpenSourceFileCommand = "employees";

            /// <summary>
            /// Command to view a custom path to a file sourceЫ
            /// </summary>
            private const string ShowFilePathCommand = "print_path";

            /// <summary>
            /// Command to setup a custom file source path
            /// </summary>
            private const string EditFilePathCommand = "path";

            /// <summary>
            /// Get console commands
            /// </summary>
            /// <returns> List of console commands </returns>
            public List<ConsoleCommand> GetCommands()
            {
                return new List<ConsoleCommand>()
                {
                    new (ShowFilePathCommand, $" * {ShowFilePathCommand}: вывести путь к файлу со списком сотрудников;", 3),
                    new (EditFilePathCommand, $" * {EditFilePathCommand}: изменить путь к файлу со списком сотрудников;", 5),
                    new(OpenSourceFileCommand, $" * {OpenSourceFileCommand}: открыть файл со списком сотрудников.", 4)
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
                        case OpenSourceFileCommand:
                            DepartmentFileProviderSettings.LaunchStorageFile();
                            break;
                        case ShowFilePathCommand:
                            DepartmentFileProviderSettings.PrintPath();
                            break;
                        case EditFilePathCommand:
                            Console.WriteLine("Пожалуйста, укажите полный путь до файла со списком сотрудников (*.json).");
                            var path = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(path))
                            {
                                Log.Warning("Путь до файла не может быть пустым.");
                                break;
                            }

                            DepartmentFileProviderSettings.UpdateFilePath(path);
                            break;
                    }

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Class settings
        /// </summary>
        internal static class DepartmentFileProviderSettings
        {
            /// <summary>
            /// Default path to a department info file
            /// </summary>
            private static readonly string DefaultSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "department.json");

            /// <summary>
            /// Custom user path to a department info file
            /// </summary>
            private static string? _sourcePath = DefaultSourcePath;

            /// <summary>
            /// Parameter-less constructor
            /// </summary>
            static DepartmentFileProviderSettings()
            {
                if (!string.IsNullOrWhiteSpace(Settings.Default.DepartmentStorageSourcePath))
                {
                    SourcePath = Settings.Default.DepartmentStorageSourcePath;
                    ValidateUserFile();
                }
            }

            /// <summary>
            /// Path to a file source
            /// </summary>
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

            /// <summary>
            /// Open storage file
            /// </summary>
            internal static void LaunchStorageFile()
            {
                ValidateUserFile();
                if (!string.IsNullOrWhiteSpace(SourcePath))
                {
                    FileUtils.LaunchFile(SourcePath);
                }
            }

            /// <summary>
            /// Update user path to a source file
            /// </summary>
            /// <param name="filePath"> New path </param>
            internal static void UpdateFilePath(string? filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath) || filePath == SourcePath)
                {
                    return;
                }

                SourcePath = filePath;
                ValidateUserFile();
            }

            /// <summary>
            /// Print user path
            /// </summary>
            internal static void PrintPath()
            {
                Console.WriteLine(SourcePath);
                Console.WriteLine();
            }

            /// <summary>
            /// Check and validate an user source path
            /// </summary>
            private static void ValidateUserFile()
            {
                if (!File.Exists(SourcePath))
                {
                    if (SourcePath == null || !SourcePath.Equals(DefaultSourcePath))
                    {
                        Log.Warning("Указанный файл со списком сотрудников не найден. Будет использован файл по умолачнию");
                    }

                    if (!File.Exists(DefaultSourcePath))
                    {
                        File.WriteAllBytes(DefaultSourcePath, Resource.SampleDepartmentFile);
                    }

                    SourcePath = DefaultSourcePath;
                }
            }
        }
    }
}