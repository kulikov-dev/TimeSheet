using System.Diagnostics;

namespace TimeSheet.Application.Utils
{
    /// <summary>
    /// Utils to work with files/explorer
    /// </summary>
    internal static class FileUtils
    {
        /// <summary>
        /// Show file in explorer
        /// </summary>
        /// <param name="path"> File path </param>
        internal static void ShowInExplorer(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var argument = "/select, \"" + path + "\"";
            Process.Start("explorer.exe", argument);
        }

        /// <summary>
        /// Launch file or open it on Explorer
        /// </summary>
        /// <param name="path"> File path </param>
        internal static void LaunchFile(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch
            {
                ShowInExplorer(path);
            }
        }
    }
}