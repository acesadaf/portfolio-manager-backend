using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using portfoliobackend.Models;
using MySql.Data.MySqlClient;

namespace portfoliobackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IDataStore _dataStore;

        public StocksController(IDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        //private static readonly List<StockModel> _myStocks = new List<StockModel>
        //{
        //    new StockModel
        //    {
        //        CurrentPrice = 45,
        //        Name = "Amazon",
        //        Ticker = "AMZN"
        //    },
        //    new StockModel
        //    {
        //        CurrentPrice = 150,
        //        Name = "Microsoft",
        //        Ticker = "MSFT"
        //    }
        //};

        // GET api/stocks/amzn
        [HttpGet()]
        public ActionResult<List<StockModel>> GetAll()
        {
            return _dataStore.GetStock(string.Empty);
            //return stock 
            //if (stock != null)
            //{
            //    return stock;
            //}

            //return NotFound();
        }

        [HttpGet("{ticker}")]
        public ActionResult<List<StockModel>> Get(string ticker)
        {
            return NotFound();
        }

        // PUT api/stocks/amzn
        [HttpPut("{ticker}")]
        public ActionResult<StockModel> Put(string ticker, [FromBody] StockModel stockModel)
        {
            if (ticker != stockModel.Ticker)
            {
                return BadRequest("Ticker in URL must match ticker in body");
            }

            _dataStore.AddStock(stockModel);

            return stockModel;
        }
        [HttpDelete()]
        public ActionResult<StockModel> Delete([FromBody] StockModel stockModel)
        {
            _dataStore.DeleteStock(stockModel);

            return stockModel;
        }
    }
}