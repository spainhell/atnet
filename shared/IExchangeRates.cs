using System;
using System.Collections.Generic;
using System.Text;
using shared;

namespace wpfapp
{
    public interface IExchangeRates
    {
        List<ForeignCurrency> Get();
    }
}
