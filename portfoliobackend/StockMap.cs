using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace portfoliobackend
{
    public class StockMap
    {
        public StockMap(int stockId, string ticker, string companyName)
        {
            StockId = stockId;
            Ticker = ticker ?? throw new ArgumentNullException(nameof(ticker));
            CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        }

        public int StockId { get; set; }

        public string Ticker { get; set; }

        public string CompanyName { get; set; }
    }
}
