using TimeSheet.Application.Logger;

namespace TimeSheet.Application
{
    internal static class Settings
    {
        public static readonly string DefaultWorkersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workers.json");

        public static string WorkersPath = DefaultWorkersPath;

        public static void ValidateWorkersFile()
        {
            if (!File.Exists(Settings.WorkersPath))
            {
                if (!Settings.WorkersPath.Equals(Settings.DefaultWorkersPath))
                {
                    Log.Warning("Указанный файл со списком сотрудников не найден. Будет использован файл по умолачнию");
                }

                if (!File.Exists(Settings.DefaultWorkersPath))
                {
                    File.Create(Settings.DefaultWorkersPath);
                }

                Settings.WorkersPath = Settings.DefaultWorkersPath; //TODO save
            }
        }
    }
}
