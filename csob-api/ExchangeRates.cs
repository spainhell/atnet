using System;
using System.Net.Http;


namespace wpfapp
{
    public class ExchangeRates : IExchangeRates
    {
        private readonly HttpClient _client;
        public string ApiName { get; private set; }

        public ExchangeRates(HttpClient client)
        {
            ApiName = "Èeskoslovenská obchodní banka";
            _client = client;
        }

        public void Get()
        {
            // ziskame json data
            var response = _client.GetAsync($"https://www.csob.cz/portal/lide/kurzovni-listek/-/date/kurzy.txt");
            var respJson = response.Result.Content.ReadAsStringAsync().Result;

            //var a = JObject.Parse(respJson);
            //var accountStatement = a.SelectToken("accountStatement");
            //var info = accountStatement.SelectToken("info");
            //var transactionList = accountStatement.SelectToken("transactionList");
            //var transactions = transactionList.SelectTokens("transaction");

            //dynamic b = JsonConvert.DeserializeObject<dynamic>(respJson);
        }
    }
}
