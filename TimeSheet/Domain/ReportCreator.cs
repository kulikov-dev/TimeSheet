using OfficeOpenXml;
using System.Globalization;
using OfficeOpenXml.Style;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Domain.Providers;
using TimeSheet.Application.Structs;
using TimeSheet.Application.Utils;

namespace TimeSheet.Domain
{
    public static class ReportCreator
    {
        /// <summary>
        /// Create a time-sheet report
        /// </summary>
        /// <param name="date"> Date of report </param>
        /// <param name="departmentProvider"> Department storage provider </param>
        /// <param name="weekendsProvider"> Weekends provider </param>
        /// <returns> Flag of success </returns>
        public static async Task<bool> Create(DateTime date, IDepartmentProvider departmentProvider, IWeekendsProvider weekendsProvider)
        {
            var dataStorage = new WeekendsStorage(weekendsProvider);
            var currentMonthWeekends = await dataStorage.GetMonthWeekends(date);

            var departmentStorage = new DepartmentStorage(departmentProvider);
            await departmentStorage.Load();

            if (!departmentStorage.HasEmployees)
            {
                Log.Warning("Отсутствуют сотрудники для формирования табеля учета рабочих дней.");
                return false;
            }

            var path = GetReportUniquePath(date);
            try
            {
                using var excel = new ExcelPackage(new FileInfo(path));
                var cultureInfo = CultureInfo.CreateSpecificCulture("ru");
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var fullMonthName = date.ToString("MMMM", cultureInfo).ToLower();

                const int startDataRow = 6;
                const int serviceColumnsCount = 2;
                const int serviceRowsCount = 3;
                var columnsCount = daysInMonth + serviceColumnsCount;
                var rowsCount = departmentStorage.EmployeesCount + serviceRowsCount;

                var worksheet = excel.Workbook.Worksheets.Add("Табель учета");
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                var cell = worksheet.Cells[3, 1];
                cell.Value = departmentStorage.DepartmentTitle;
                cell.Style.Font.Size = 12;
                worksheet.Cells[3, 1, 3, columnsCount].Merge = true;

                cell = worksheet.Cells[4, 1];
                cell.Value = $"Табель учета рабочего времени за {fullMonthName} {date.Year} года.";
                cell.Style.Font.Size = 12;
                worksheet.Cells[4, 1, 4, columnsCount].Merge = true;

                cell = worksheet.Cells[startDataRow, 1];
                cell.Value = "№ Ф.И.О.";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                cell = worksheet.Cells[startDataRow, 2];
                cell.Value = "Числа месяца";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                cell = worksheet.Cells[startDataRow, columnsCount];
                cell.Value = "Кол-во рабочих дней";
                cell.Style.WrapText = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var dayNames = cultureInfo.DateTimeFormat.AbbreviatedDayNames.Select(dayName => dayName.ToLower()).ToList();
                var employeeIndex = -1;
                foreach (var worker in departmentStorage)
                {
                    ++employeeIndex;
                    var currentRowPosition = startDataRow + employeeIndex + serviceRowsCount;
                    worksheet.Cells[currentRowPosition, 1].Value = worker.FullName;

                    for (var j = 1; j <= daysInMonth; j++)
                    {
                        var dayName = dayNames[(int)new DateTime(date.Year, date.Month, j).DayOfWeek];
                        if (employeeIndex == 0)
                        {
                            //// Day index
                            cell = worksheet.Cells[startDataRow + 1, j + 1];
                            cell.Value = j.ToString();
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            //// Day name
                            cell = worksheet.Cells[startDataRow + 2, j + 1];
                            cell.Value = dayName;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        if (currentMonthWeekends.Contains(j))
                        {
                            continue;
                        }

                        cell = worksheet.Cells[currentRowPosition, j + 1];
                        cell.Value = worker.WorkHours;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    cell = worksheet.Cells[currentRowPosition, columnsCount];
                    cell.Formula = $"SUM({worksheet.Cells[currentRowPosition, serviceColumnsCount, currentRowPosition, columnsCount - 1]})";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells[startDataRow, columnsCount, startDataRow + departmentStorage.EmployeesCount, columnsCount].Style.Font.Size = 11;
                cell = worksheet.Cells[startDataRow + 1 + rowsCount, 1];
                cell.Value = "к - командировка, б - больничный, о - отпуск, п - прогул, д - декрет, 8 - проработанное время, у - увольнения";
                cell.Style.Font.Size = 10;
                cell.Style.Font.Italic = true;

                worksheet.Cells[startDataRow + rowsCount + 1, 1].Style.Font.Italic = true;

                var range = worksheet.Cells[startDataRow, 1, startDataRow + 2, 1];
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Merge = true;

                range = worksheet.Cells[startDataRow, 2, startDataRow, columnsCount - 1];
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Merge = true;

                range = worksheet.Cells[startDataRow, columnsCount, startDataRow + 2, columnsCount];
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Merge = true;

                var leaderColumnPosition = columnsCount / 2;
                worksheet.Cells[startDataRow + rowsCount + 4, leaderColumnPosition].Value = departmentStorage.LeaderPosition;
                worksheet.Cells[startDataRow + rowsCount + 4, leaderColumnPosition].Style.Font.Size = 11;

                range = worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var signatureRowPosition = startDataRow + rowsCount + startDataRow;
                cell = worksheet.Cells[signatureRowPosition, leaderColumnPosition];
                cell.Value = @"__________________________";
                cell.Style.Font.Size = 11;

                var signatureColumnPosition = (int)(columnsCount * 0.8f);
                worksheet.Cells[signatureRowPosition, signatureColumnPosition].Value = departmentStorage.LeaderName;
                worksheet.Cells[signatureRowPosition, signatureColumnPosition].Style.Font.Size = 11;

                worksheet.Column(1).Width = 13.86;
                worksheet.Column(columnsCount).Width = 8.43;
                for (var j = serviceColumnsCount; j <= serviceColumnsCount + daysInMonth - 1; j++)
                {
                    worksheet.Column(j).Width = 2.71;
                }

                excel.Save();
            }
            catch (Exception ex)
            {
                Log.Error("При создании отчета произошла ошибка. Пожалуйста, обратитесь к разработчикам.", ex);
                return false;
            }

            FileUtils.LaunchFile(path);

            Log.Info("Создание отчета успешно завершено.");
            return true;
        }

        /// <summary>
        /// Get unique path for a created report
        /// </summary>
        /// <param name="date"> Report date </param>
        /// <returns> Unique path </returns>
        private static string GetReportUniquePath(DateTime date)
        {
            var dateStr = date.ToShortDateString();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"report_{dateStr}.xls");
            var indexer = 1;
            while (File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"report_{dateStr}({indexer}).xls");
                indexer++;
            }

            return path;
        }

        /// <summary>
        /// Class command handler
        /// </summary>
        /// <remarks> Used by reflection. Do not remove </remarks>
        internal sealed class ReportCreatorCommandHandler : ICommandsHandler
        {
            /// <summary>
            /// Create time sheet report
            /// </summary>
            internal const string Report = "report";

            /// <summary>
            /// Get console commands
            /// </summary>
            /// <returns> List of console commands </returns>
            public List<ConsoleCommand> GetCommands()
            {
                return new List<ConsoleCommand>()
                {
                    new (Report, $" * {Report}: создать табель учета рабочего времени на текущую дату;", 1),
                    new (Report, $" * {Report} ДАТА: создать табель учета рабочего времени на указанную дату;", 2),
                };
            }

            /// <summary>
            /// Process command
            /// </summary>
            /// <param name="command"> A command </param>
            /// <returns> A command processed </returns>
            public async Task<bool> Process(string command)
            {
                var commands = GetCommands();
                foreach (var localCommand in commands)
                {
                    if (!command.StartsWith(localCommand.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    switch (localCommand.Name)
                    {
                        case Report:
                            var subCommands = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var date = DateTime.Now;
                            if (subCommands.Length == 2)
                            {
                                if (!DateTime.TryParse(subCommands[1], out date))
                                {
                                    date = DateTime.Now;
                                }
                            }

                            Console.WriteLine("Пожалуйста подождите, идет подготовка отчета...");
                            await Create(date, new DepartmentFileProvider(), new ConsultantWeekendsProvider());
                            break;
                    }
                    return true;
                }

                return false;
            }
        }
    }
}