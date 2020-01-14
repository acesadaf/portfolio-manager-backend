using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace portfoliobackend.Models
{
    public class StockModel
    {
        public string Ticker { get; set; }

        public int UserId => 2;

        public double PurchasePrice { get; set; }

        public double Quantity { get; set; }

        public DateTime PurchaseDate  { get; set; }

        public string CompanyName { get; set; }
        public double CurrentPrice { get; set; }
    }
}
