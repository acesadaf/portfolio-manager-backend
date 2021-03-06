﻿using portfoliobackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace portfoliobackend
{
    public interface IDataStore
    {
        void AddStock(StockModel stockModel);

        void DeleteStock(StockModel stockModel);

        List<StockModel> GetStock(string ticker);
    }
}
