using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;
using OfficeOpenXml.Style;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Domain.Providers;
using TimeSheet.Application.Structs;

namespace TimeSheet.Domain
{
    internal static class ReportCreator
    {
        internal static async Task Create(DateTime date, IDepartmentProvider? departmentProvider, IWeekendsProvider weekendsProvider)
        {
            var dataStorage = new WeekendsStorage(weekendsProvider);
            var currentMonthWeekends = await dataStorage.GetMonthWeekends(date);

            var departmentStorage = new DepartmentStorage(departmentProvider);
            await departmentStorage.Load();

            if (departmentStorage == null || !departmentStorage.HasWorkers)
            {
                Log.Warning("Отсутствуют сотрудники для формирования табель учета рабочих дней.");
                return;
            }

            var path = GetUniquePath(date);

            try
            {
                using (var excel = new ExcelPackage(new FileInfo(path)))
                {
                    var cultureInfo = CultureInfo.CreateSpecificCulture("ru");
                    var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                    var columnsCount = daysInMonth + 2;
                    var fullMonthName = date.ToString("MMMM", cultureInfo).ToLower();

                    const int startDataRow = 6;
                    var rowsCount = departmentStorage.WorkersCount + 3;

                    var worksheet = excel.Workbook.Worksheets.Add("Табель учета");
                    var cell = worksheet.Cells[3, 1];
                    cell.Value = departmentStorage.Department;
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
                    var range = worksheet.Cells[startDataRow, columnsCount];
                    range.Value = "Кол-во рабочих дней";
                    range.Style.WrapText = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    var names = cultureInfo.DateTimeFormat.AbbreviatedDayNames.Select(x => x.ToLower()).ToList();
                    var i = -1;
                    foreach (var worker in departmentStorage)
                    {
                        ++i;
                        var iPos = startDataRow + i + 3;
                        worksheet.Cells[iPos, 1].Value = worker.FullName;

                        var workDays = 0;
                        for (var j = 1; j <= daysInMonth; j++)
                        {
                            var dayName = names[(int)new DateTime(date.Year, date.Month, j).DayOfWeek];
                            if (i == 0)
                            {
                                cell = worksheet.Cells[startDataRow + 1, j + 1];
                                cell.Value = j.ToString();
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cell = worksheet.Cells[startDataRow + 2, j + 1];
                                cell.Value = dayName;
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }

                            if (currentMonthWeekends.Contains(j) || dayName == "сб" || dayName == "вс")
                            {
                                continue;
                            }

                            cell = worksheet.Cells[iPos, j + 1];
                            cell.Value = worker.Hours;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ++workDays;
                        }

                        cell = worksheet.Cells[iPos, columnsCount];
                        cell.Value = workDays;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    worksheet.Cells[startDataRow, columnsCount, startDataRow + departmentStorage.WorkersCount,
                        columnsCount].Style.Font.Size = 11;
                    range = worksheet.Cells[startDataRow + 1 + rowsCount, 1];
                    range.Value =
                        "к - командировка, б - больничный, о - отпуск, п - прогул, д - декрет, 8 - проработанное время, у - увольнения";
                    range.Style.Font.Size = 10;
                    range.Style.Font.Italic = true;

                    worksheet.Cells[startDataRow + rowsCount + 1, 1].Style.Font.Italic = true;
                    range = worksheet.Cells[startDataRow, 1, startDataRow + 2, 1];
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Merge = true;

                    range = worksheet.Cells[startDataRow, 2, startDataRow, columnsCount - 1];
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Merge = true;

                    range = worksheet.Cells[startDataRow, columnsCount, startDataRow + 2, columnsCount];
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Merge = true;

                    var colPos = columnsCount / 2;
                    worksheet.Cells[startDataRow + rowsCount + 4, colPos].Value = departmentStorage.Position;
                    worksheet.Cells[startDataRow + rowsCount + 4, colPos].Style.Font.Size = 11;

                    range = worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount];
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    var pos = startDataRow + rowsCount + startDataRow;
                    colPos = columnsCount / 2;

                    cell = worksheet.Cells[pos, colPos];
                    cell.Value = @"__________________________";
                    cell.Style.Font.Size = 11;

                    colPos = (int)(columnsCount * 0.8f);
                    worksheet.Cells[pos, colPos].Value = departmentStorage.Leader;
                    worksheet.Cells[pos, colPos].Style.Font.Size = 11;

                    worksheet.Column(1).Width = 13.86;
                    worksheet.Column(columnsCount).Width = 8.43;
                    for (var j = 1; j <= daysInMonth; j++)
                    {
                        worksheet.Column(j + 1).Width = 2.71;
                    }

                    excel.Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error("При создании отчета произошла ошибка. Пожалуйста обратитесь к разработчикам.", ex);
                return;
            }

            try
            {
                Process.Start(path);
            }
            catch
            {
                var argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
            }


            Log.Info("Создание отчета успешно завершено.");
        }

        private static string GetUniquePath(DateTime date)
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
        /// 
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
                    new (Report, $" * {Report}: создать табель учета рабочего времени;", 1)
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
                    if (localCommand.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase))
                    {
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

                                Console.WriteLine($"Пожалуйста подождите, идет подготовка отчета//");
                                await ReportCreator.Create(date, new DepartmentFileProvider(), new ConsultantWeekendsProvider());
                                break;
                        }
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
