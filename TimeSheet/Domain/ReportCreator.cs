using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using TimeSheet.Application;
using TimeSheet.Application.Data;
using TimeSheet.Application.Logger;
using TimeSheet.Application.Providers;

namespace TimeSheet.Domain
{
    internal static class ReportCreator
    {
        public static async Task Create(DateTime date)
        {
            var dataStorage = new WeekendsStorage(new ConsultantWeekendsProvider());
            var currentMonthWeekends = await dataStorage.GetMonthWeekends(date);

            using var fileStream = new FileStream(Settings.WorkersPath, FileMode.Open, FileAccess.Read);

            var data = await JsonSerializer.DeserializeAsync<DepartmentInfo>(fileStream);
            if (data == null)
            {
                Log.Error("Не удалось загрузить данные по сотрудникам. Проверьте корректность файла данных");
                return;
            }

            var path = GetUniquePath(date);
            var cell = default(ExcelRange);
            var range = default(ExcelRange);

            using (var excel = new ExcelPackage(new FileInfo(path)))
            {
                var cultureInfo = CultureInfo.CreateSpecificCulture("ru");
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var columnsCount = daysInMonth + 2;
                var fullMonthName = date.ToString("MMMM", cultureInfo).ToLower();

                const int startDataRow = 6;
                var rowsCount = data.Employers.Count + 3;

                var worksheet = excel.Workbook.Worksheets.Add("Табель учета");
                worksheet.Cells[3, 1].Value = data.Department;
                worksheet.Cells[3, 1].Style.Font.Size = 12;
                worksheet.Cells[3, 1, 3, columnsCount].Merge = true;

                worksheet.Cells[4, 1].Value = $"Табель учета рабочего времени за {fullMonthName} {date.Year} года.";
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[4, 1, 4, columnsCount].Merge = true;

                worksheet.Cells[startDataRow, 1].Value = "№ Ф.И.О.";
                worksheet.Cells[startDataRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[startDataRow, 2].Value = "Числа месяца";
                worksheet.Cells[startDataRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range = worksheet.Cells[startDataRow, columnsCount];
                range.Value = "Кол-во рабочих дней";
                range.Style.WrapText = true;
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                var names = cultureInfo.DateTimeFormat.AbbreviatedDayNames.Select(x => x.ToLower()).ToList();
                for (var i = 0; i < data.Employers.Count; ++i)
                {
                    var iPos = startDataRow + i + 3;
                    var emp = data.Employers[i];
                    worksheet.Cells[iPos, 1].Value = emp.FullName;

                    var workDays = 0;
                    for (var j = 1; j <= daysInMonth; j++)
                    {
                        var dayName = names[(int)new DateTime(date.Year, date.Month, j).DayOfWeek];
                        if (i == 0)
                        {
                            worksheet.Cells[startDataRow + 1, j + 1].Value = j.ToString();
                            worksheet.Cells[startDataRow + 1, j + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells[startDataRow + 2, j + 1].Value = dayName;
                            worksheet.Cells[startDataRow + 2, j + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        if (currentMonthWeekends.Contains(j) || dayName == "сб" || dayName == "вс")
                        {
                            continue;
                        }

                        cell = worksheet.Cells[iPos, j + 1];
                        cell.Value = emp.Hours;
                        cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ++workDays;
                    }

                    cell = worksheet.Cells[iPos, columnsCount];
                    cell.Value = workDays;
                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells[startDataRow, columnsCount, startDataRow + data.Employers.Count, columnsCount].Style.Font.Size = 11;
                worksheet.Cells[startDataRow + 1 + rowsCount, 1].Value = "к - командировка, б - больничный, о - отпуск, п - прогул, д - декрет, 8 - проработанное время, у - увольнения";
                worksheet.Cells[startDataRow + 1 + rowsCount, 1].Style.Font.Size = 10;
                worksheet.Cells[startDataRow + 1 + rowsCount, 1].Style.Font.Italic = true;

                worksheet.Cells[startDataRow + rowsCount + 1, 1].Style.Font.Italic = true;
                worksheet.Cells[startDataRow, 1, startDataRow + 2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[startDataRow, 1, startDataRow + 2, 1].Merge = true;

                worksheet.Cells[startDataRow, 2, startDataRow, columnsCount - 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[startDataRow, 2, startDataRow, columnsCount - 1].Merge = true;

                worksheet.Cells[startDataRow, columnsCount, startDataRow + 2, columnsCount].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[startDataRow, columnsCount, startDataRow + 2, columnsCount].Merge = true;

                var colPos = columnsCount / 2;
                worksheet.Cells[startDataRow + rowsCount + 4, colPos].Value = data.Position;
                worksheet.Cells[startDataRow + rowsCount + 4, colPos].Style.Font.Size = 11;

                worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[startDataRow, 1, startDataRow + rowsCount - 1, columnsCount].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                var pos = startDataRow + rowsCount + startDataRow;
                colPos = columnsCount / 2;

                worksheet.Cells[pos, colPos].Value = @"__________________________";
                worksheet.Cells[pos, colPos].Style.Font.Size = 11;
                colPos = (int)(columnsCount * 0.8f);
                worksheet.Cells[pos, colPos].Value = data.Leader;
                worksheet.Cells[pos, colPos].Style.Font.Size = 11;

                worksheet.Column(1).Width = 13.86;
                worksheet.Column(columnsCount).Width = 8.43;
                for (var j = 1; j <= daysInMonth; j++)
                {
                    worksheet.Column(j + 1).Width = 2.71;
                }

                excel.Save();
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
    }
}
