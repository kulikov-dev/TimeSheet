using System.Text;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using TimeSheet.Application.Interfaces;

namespace TimeSheet.Application.Logger
{
    /// <summary>
    /// Log wrapper to output with NLog
    /// </summary>
    internal sealed class NLogWrapper : ILogWrapper
    {
        /// <summary>
        /// NLog logger
        /// </summary>
        private readonly NLog.Logger _logger;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public NLogWrapper()
        {
            LogFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            var config = new LoggingConfiguration();
            var csvTarget = new FileTarget
            {
                Encoding = Encoding.UTF8,
                FileName = Path.Combine(LogFolderPath, "csv-${shortdate}.log"),
                ArchiveFileName = Path.Combine(LogFolderPath, "log.{#}.txt"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 4,
                ConcurrentWrites = false
            };

            var csvLayout = new CsvLayout
            {
                Delimiter = CsvColumnDelimiterMode.Tab,
                WithHeader = true
            };
            csvLayout.Columns.Add(new CsvColumn("time", "${longdate}"));
            csvLayout.Columns.Add(new CsvColumn("level", "${level:upperCase=true}"));
            csvLayout.Columns.Add(new CsvColumn("message", "${message}"));
            csvLayout.Columns.Add(new CsvColumn("stacktrace", "${stacktrace:topFrames=10}"));
            csvLayout.Columns.Add(new CsvColumn("exception", "${exception:format=ToString}"));
            csvTarget.Layout = csvLayout;

            config.AddTarget("csv-file", csvTarget);
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Path to a log folder
        /// </summary>
        public string LogFolderPath { get; }

        /// <summary>
        /// Echo information message
        /// </summary>
        /// <param name="message"> Message </param>
        public void Info(string message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// Echo warning message
        /// </summary>
        /// <param name="message"> Message </param>
        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        /// <summary>
        /// Echo error message
        /// </summary>
        /// <param name="message"> Message </param>
        /// <param name="ex"> Exception </param>
        public void Error(string message, Exception? ex = null)
        {
            if (ex == null)
            {
                _logger.Error(message);
            }
            else
            {
                _logger.Error(ex, message);
            }
        }
    }
}
