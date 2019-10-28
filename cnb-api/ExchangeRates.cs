using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace wpfapp
{
    class ExchangeRates : IExchangeRates
    {
        private readonly HttpClient _client;
        public string ApiName { get; private set; }

        public ExchangeRates(HttpClient client)
        {
            ApiName = "Česká národní banka";
            _client = client;
        }

        public void Get()
        {
            // ziskame json data
            var response = _client.GetAsync($"https://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt");
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
