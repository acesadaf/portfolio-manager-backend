using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using portfoliobackend.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace portfoliobackend
{
    public class DataStore : IDataStore
    {
        string connectionString = @"Server=localhost;Database=aspcrud;Uid=root;Pwd=yoloswag;";

        private static readonly string readCommand = "SELECT * FROM StockData WHERE UserId = 2";
        private static readonly string writeCommand = "INSERT INTO StockData(StockId, UserId, Qty, PurchasePrice, PurchaseDate) values (@stockId, @userId, @qty, @purchasePrice, @purchaseDate)";
        private static readonly string addToMapCommand = "INSERT INTO stockmap(CompanyName, Ticker) values (@CompanyName, @Ticker)";
        private IDictionary<string, StockMap> _tickerMap;
        private IDictionary<int, string> _id2Ticker;
        private readonly IDictionary<int, string> inverseTickerMap;
        public static string queriedCompanyName = "none";
        public static double queriedCompanyPrice = 0;
        public static bool FetchDone = false; 


        public DataStore()
        {
            _tickerMap = LoadStockMap();
            _id2Ticker = _tickerMap.ToDictionary(r => r.Value.StockId, r => r.Key);

            inverseTickerMap = new Dictionary<int, string>();
            foreach (KeyValuePair<string, StockMap> tMap in _tickerMap)
            {
                inverseTickerMap.Add(tMap.Value.StockId, tMap.Key);
            }
        }

        public void AddToMap (string companyName, string ticker)
        {
            _ = ExecuteSqlCommand(sqlCmd =>
            {
                sqlCmd.Parameters.Add(new MySqlParameter("@CompanyName", MySqlDbType.VarChar) { Value = companyName });
                sqlCmd.Parameters.Add(new MySqlParameter("@Ticker", MySqlDbType.VarChar) { Value = ticker });
                return sqlCmd.ExecuteNonQuery();
            }, addToMapCommand);
        }

        public static async void GetStockData(string ticker)
        {
            Console.WriteLine("HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            string baseURL = $"https://api.worldtradingdata.com/api/v1/stock?symbol={ticker}&api_token=WwG42fcH0mrQapxcera6JlzsQMdFO4XVDrAW2QsBTvsyNgEHTIcRBEHdWO0b";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseURL))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            if (data != null)
                            {
                                //Parse your data into a object.
                                var dataObj = JObject.Parse(data);
                                //Then create a new instance of PokeItem, and string interpolate your name property to your JSON object.
                                //Which will convert it to a string, since each property value is a instance of JToken.
                                MyApiData thisStock = new MyApiData(ticker: (string)dataObj.SelectToken("data[0].symbol"), companyName: (string)dataObj.SelectToken("data[0].name"), price: Convert.ToDouble((string)dataObj.SelectToken("data[0].price")));
                                Console.WriteLine("Pokemon Name: {0}", thisStock.CompanyName);
                                queriedCompanyName = thisStock.CompanyName;
                                queriedCompanyPrice = thisStock.Price;
                                //queriedCompanyName = "NETFLIX";
                            }
                            else
                            {
                                Console.WriteLine("Data is null!");
                                //queriedCompanyName = "NETFLIX";
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            FetchDone = true;
        }

        public void AddStock(StockModel stockModel)
        {
            _ = ExecuteSqlCommand(
                sqlCmd =>
                {
                    var stockId = 1;
                    bool failed = false; 
                    try
                    {
                        stockId = _tickerMap[stockModel.Ticker].StockId;
                    }
                    catch (KeyNotFoundException e)
                    {
                        failed = true;
                        Console.WriteLine(queriedCompanyName);
                        GetStockData(stockModel.Ticker);
                        while(FetchDone!=true)
                        {

                        }
                        FetchDone = false;
                        Console.WriteLine(queriedCompanyName);
                        AddToMap(queriedCompanyName, stockModel.Ticker);
                        _tickerMap = LoadStockMap();
                        _id2Ticker = _tickerMap.ToDictionary(r => r.Value.StockId, r => r.Key);
                    }
                    finally
                    {
                        if (failed) stockId = _tickerMap[stockModel.Ticker].StockId;
                    }
                    
                    sqlCmd.Parameters.Add(new MySqlParameter("@stockId", MySqlDbType.Int32) { Value = stockId });
                    sqlCmd.Parameters.Add(new MySqlParameter("@userId", MySqlDbType.Int32) { Value = stockModel.UserId });
                    sqlCmd.Parameters.Add(new MySqlParameter("@qty", MySqlDbType.Double) { Value = stockModel.Quantity });
                    sqlCmd.Parameters.Add(new MySqlParameter("@purchasePrice", MySqlDbType.Double) { Value = stockModel.PurchasePrice });
                    sqlCmd.Parameters.Add(new MySqlParameter("@purchaseDate", MySqlDbType.DateTime) { Value = stockModel.PurchaseDate });

                    return sqlCmd.ExecuteNonQuery();
                },
                writeCommand);
        }

        public void DeleteStock(string ticker)
        {
            throw new NotImplementedException();
        }

        public List<StockModel> GetStock(string ticker)
        {
            return ExecuteSqlCommand(
                sqlCmd =>
                {
                    List<StockModel> stockList = new List<StockModel>();
                    var reader = sqlCmd.ExecuteReader();
                    while(reader.Read())
                    {
                        string Ticker = _id2Ticker[reader.GetInt32("StockId")];
                        GetStockData(Ticker);
                        while (FetchDone != true)
                        {

                        }
                        FetchDone = false;
                        stockList.Add(new StockModel
                        {
                            PurchaseDate = reader.GetDateTime("PurchaseDate"),
                            PurchasePrice = reader.GetDouble("PurchasePrice"),
                            Ticker = _id2Ticker[reader.GetInt32("StockId")],
                            Quantity = reader.GetDouble("Qty"),
                            CompanyName = queriedCompanyName,
                            CurrentPrice = queriedCompanyPrice
                }) ;
                    }

                    return stockList;
                },
                readCommand);
            //throw new NotImplementedException();
        }

        private Dictionary<string, StockMap> LoadStockMap()
        {
            const string stockMapReadCommand = "SELECT * FROM stockmap";
            return ExecuteSqlCommand(
                cmd =>
                {
                    var dict = new Dictionary<string, StockMap>(StringComparer.OrdinalIgnoreCase);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var ticker = reader.GetString("Ticker");
                        var companyName = reader.GetString("CompanyName");
                        var stockId = reader.GetInt32("stockId");

                        dict.Add(ticker, new StockMap(stockId, ticker, companyName));
                    }

                    return dict;
                },
                stockMapReadCommand);
        }

        private T ExecuteSqlCommand<T>(Func<MySqlCommand, T> function, string cmd)
        {
            using (MySqlConnection sqlCon = new MySqlConnection(connectionString))
            using (MySqlCommand sqlCom = new MySqlCommand(cmd, sqlCon))
            {
                sqlCon.Open();
                Console.WriteLine("HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                return function(sqlCom);
            }
        }
    }
}
