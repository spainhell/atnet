using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class ForeignCurrency
    {
        public DateTime Datum { get; set; }
        public string Mena { get; set; }
        public string Zeme { get; set; }
        public decimal Mnozstvi { get; set; }
        public decimal DevizaNakup { get; set; }
        public decimal DevizaProdej { get; set; }
    }
}