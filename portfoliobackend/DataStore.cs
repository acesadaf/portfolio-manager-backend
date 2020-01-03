using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using portfoliobackend.Models;

namespace portfoliobackend
{
    public class DataStore : IDataStore
    {
        string connectionString = @"Server=localhost;Database=aspcrud;Uid=root;Pwd=yoloswag;";

        private static readonly string readCommand = "SELECT * FROM StockData WHERE StockId = @stockId";
        private static readonly string writeCommand = "INSERT INTO StockData(StockId, UserId, Qty, PurchasePrice, PurchaseDate) values (@stockId, @userId, @qty, @purchasePrice, @purchaseDate)";

        private readonly IDictionary<string, StockMap> _tickerMap;

        public DataStore()
        {
            _tickerMap = LoadStockMap();
        }

        public void AddStock(StockModel stockModel)
        {
            _ = ExcecuteSqlCommand(
                sqlCmd =>
                {
                    var stockId = _tickerMap[stockModel.Ticker].StockId;

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

        public StockModel GetStock(string ticker)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, StockMap> LoadStockMap()
        {
            const string stockMapReadCommand = "SELECT * FROM stockmap";
            return ExcecuteSqlCommand(
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

        private T ExcecuteSqlCommand<T>(Func<MySqlCommand, T> function, string cmd)
        {
            using (MySqlConnection sqlCon = new MySqlConnection(connectionString))
            using (MySqlCommand sqlCom = new MySqlCommand(cmd, sqlCon))
            {
                sqlCon.Open();

                return function(sqlCom);
            }
        }
    }
}
