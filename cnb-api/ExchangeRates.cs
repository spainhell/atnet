using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using shared;

namespace wpfapp
{
    class ExchangeRates : IExchangeRates
    {
        private readonly HttpClient _client;
        public string ApiName { get; private set; }

        public ExchangeRates(HttpClient client)
        {
            ApiName = "ČNB";
            _client = client;
        }

        public List<ForeignCurrency> Get()
        {
            List<ForeignCurrency> list = new List<ForeignCurrency>();
            // ziskame json data
            var response = _client.GetAsync($"https://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt");
            var cnbTxt = response.Result.Content.ReadAsStringAsync().Result;

            string[] lines = cnbTxt.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lines.Length > 2)
            {
                DateTime dt = DateTime.Parse(lines[0].Substring(0, lines[0].IndexOf(" ") + 1));
                for (int i = 2; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split("|");
                    if (items.Length < 5) continue;

                    var zeme = items[0];
                    var mena = items[1];
                    var mnozstvi = Convert.ToDecimal(items[2]);
                    var devizaNakup = Convert.ToDecimal(items[4]);
                    var devizaProdej = devizaNakup;

                    var fc = new ForeignCurrency()
                        { Datum = dt, Zeme = zeme, Mena = mena, Mnozstvi = mnozstvi, DevizaNakup = devizaNakup, DevizaProdej = devizaProdej };
                    list.Add(fc);
                }
            }

            return list;
        }
    }
}