using AngleSharp.Html.Parser;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;
using TimeSheet.Data;

namespace TimeSheet.Domain.Providers
{
    internal class ConsultantWeekendsProvider : IWeekendsProvider
    {
        private const string constUrl = "http://www.consultant.ru/law/ref/calendar/proizvodstvennye/";

        public async Task<AnnualWeekendsInfo> GetAnnualWeekends(DateTime date)
        {
            var url = $"{constUrl}{date.Year}/";
            var result = new AnnualWeekendsInfo(date.Year);
            try
            {
                using (var webClient = new HttpClient())
                {
                    var html = await webClient.GetStringAsync(url);

                    var parser = new HtmlParser();
                    var document = await parser.ParseDocumentAsync(html);
                    var selector = document.QuerySelectorAll(".cal");
                    for (var i = 0; i < selector.Length; i++)
                    {
                        var weekendSelector = selector[i].QuerySelectorAll(".holiday");
                        result.AddMonthWeekends(i, weekendSelector.Select(item => int.Parse(item.InnerHtml)).ToList());
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось получить данные по выходным днмя с сервера Консультант Плюс", ex);
                return result;
            }
        }
    }
}
