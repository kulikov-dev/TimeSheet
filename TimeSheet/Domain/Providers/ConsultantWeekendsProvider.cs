using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Data;

namespace TimeSheet.Domain.Providers
{
    /// <summary>
    /// Provider for taking weekends/holidays from the Consultant Plus source
    /// </summary>
    internal class ConsultantWeekendsProvider : IWeekendsProvider
    {
        /// <summary>
        /// Path to the source url
        /// </summary>
        private const string ConsultantUrl = "http://www.consultant.ru/law/ref/calendar/proizvodstvennye/";

        /// <summary>
        /// Get annual weekends
        /// </summary>
        /// <param name="date"> Date </param>
        /// <returns> Annual weekends </returns>
        public async Task<AnnualWeekendsInfo> GetAnnualWeekends(DateTime date)
        {
            var monthUrl = $"{ConsultantUrl}{date.Year}/";
            var result = new AnnualWeekendsInfo(date.Year);
            try
            {
                string? html;
                using (var webClient = new HttpClient())
                {
                    html = await webClient.GetStringAsync(monthUrl);
                }

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(html);
                var selector = document.QuerySelectorAll(".cal");
                for (var i = 0; i < selector.Length; i++)
                {
                    var weekendSelector = selector[i].QuerySelectorAll(".holiday,.weekend").Where(x => x is IHtmlTableDataCellElement);
                    result.AddMonthWeekends(i + 1, weekendSelector.Select(item => int.Parse(item.InnerHtml)).ToList());
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось получить данные по выходным дням с сервера Консультант Плюс.", ex);
                return result;
            }
        }
    }
}