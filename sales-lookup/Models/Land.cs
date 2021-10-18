using System;
using System.Collections.Generic;
using System.Text;

namespace sales_lookup.Models
{
    public class Land
    {
        public int X { get; set; }

        public int Y { get; set; }

        public bool ForSale { get; set; }

        public decimal SalesPrice { get; set; }

        public bool RecentlySold => RecentlySoldPrices.Count > 0;

        public List<decimal> RecentlySoldPrices { get; set; }
        public bool IsPlaza { get; internal set; }

        public Land()
        {
            RecentlySoldPrices = new List<decimal>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Pavia ");
            sb.Append(X);
            sb.Append(" ");
            sb.Append(Y);
            sb.Append(" ");

            if (ForSale)
            {
                sb.Append(" sales price: ");
                sb.Append(SalesPrice / 1000000); // cnft api price is in lovelace
                sb.Append(" ADA");
            }

            return sb.ToString();
        }
    }
}
