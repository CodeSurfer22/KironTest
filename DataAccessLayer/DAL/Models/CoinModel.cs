using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CoinModel
    {
        public string Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Rank { get; set; }
        public decimal Price { get; set; }
        public decimal PriceBtc { get; set; }
        public decimal Volume { get; set; }
        public decimal MarketCap { get; set; }
        public decimal AvailableSupply { get; set; }
        public decimal TotalSupply { get; set; }
        public decimal FullyDilutedValuation { get; set; }
        public decimal PriceChange1h { get; set; }
        public decimal PriceChange1d { get; set; }
        public decimal PriceChange1w { get; set; }
        public string RedditUrl { get; set; }
        public string TwitterUrl { get; set; }
        public List<string> Explorers { get; set; }
        public string WebsiteUrl { get; set; } // Ethereum-specific
        public string ContractAddress { get; set; } // Ethereum-specific
        public int? Decimals { get; set; } // Ethereum-specific
    }


}
