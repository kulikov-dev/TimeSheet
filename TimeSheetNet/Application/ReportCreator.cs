using AngleSharp.Html.Parser;
using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace TimeSheetNet.Application
{
    internal static class ReportCreator
    {
        public static async Task Create(DateTime date)
        {
            var weekends = await GetOfficialWeekends(date.Year);
            var currentMonthWeekends = weekends[date.Month - 1];


            using var fileStream = new FileStream(Settings.WorkersPath, FileMode.Open, FileAccess.Read);

            //async version
            var data = await JsonSerializer.DeserializeAsync<Data>(fileStream);

            if (data == null)
            {
                Console.WriteLine("долбоеб");
                return;
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"report_{date.ToShortDateString()}.xls");
            var indexer = 1;
            while (File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"report_{date.ToShortDateString()}({indexer}).xls");
                indexer++;
            }

            using (var excel = new ExcelPackage(new FileInfo(path)))
            {
                var cultureInfo = CultureInfo.CreateSpecificCulture("ru");
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var columnsCount = daysInMonth + 2;
                var fullMonthName = date.ToString("MMMM", cultureInfo).ToLower();

                const int startDataRow = 6;
                var rowsCount = data.Employers.Count + 3;


                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Табель учета");
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
                worksheet.Cells[startDataRow, columnsCount].Value = "Кол-во рабочих дней";
                worksheet.Cells[startDataRow, columnsCount].Style.WrapText = true;
                worksheet.Cells[startDataRow, columnsCount].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[startDataRow, columnsCount].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

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
                            continue;

                        worksheet.Cells[iPos, j + 1].Value = emp.Hours;
                        worksheet.Cells[iPos, j + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ++workDays;
                    }

                    worksheet.Cells[iPos, columnsCount].Value = workDays;
                    worksheet.Cells[iPos, columnsCount].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
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

                worksheet.Cells[startDataRow, 1, startDataRow+rowsCount-1, columnsCount].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
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
                    worksheet.Column(j+1).Width = 2.71;
                }

                excel.Save();
            }

            try
            {
                Process.Start(path);
            }
            catch
            {
                string argument = "/select, \"" + path + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private async static Task<Dictionary<int, List<int>>> GetOfficialWeekends(int year)
        {
            var url = string.Format("http://www.consultant.ru/law/ref/calendar/proizvodstvennye/{0}/", year);
            using (var webClient = new HttpClient())
            {
                var html = await webClient.GetStringAsync(url);


                var weekends = new Dictionary<int, List<int>>();

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(html);
                var selector = document.QuerySelectorAll(".cal");
                for (var i = 0; i < selector.Length; i++)
                {
                    var weekendSelector = selector[i].QuerySelectorAll(".holiday");
                    weekends.Add(i, weekendSelector.Select(x => int.Parse(x.InnerHtml)).ToList());
                }

                return weekends;
            }
        }
    }
}
