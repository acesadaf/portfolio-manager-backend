using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace portfoliobackend
{
    public class MyApiData
    {
        public MyApiData(string ticker, string companyName, double price)
        {
            Ticker = ticker;
            CompanyName = companyName;
            Price = price;
        }
        public string Ticker { get; set; }
        public string CompanyName { get; set; }
        public double Price { get; set; }
    }
}
