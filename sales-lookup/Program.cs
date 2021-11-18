using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using sales_lookup.Models;
using sales_lookup.Models.CNFT.io;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace sales_lookup
{
    class Program
    {
        static void Main(string[] args)
        {
            int centerX = AskForInt("X", -115, 108);
            int centerY = AskForInt("Y", -138, 84);
            int radius = AskForInt("radius", 2, 40);

            Console.WriteLine("Getting prices");
            Console.WriteLine();

            World world = new World();
            var coords = world.GetCoordinates(centerX, centerY);

            ProcessWorld(world.Lands,
                coords.X - (radius / 2),
                coords.X + (radius / 2) + 1,
                coords.Y - (radius / 2),
                coords.Y + (radius / 2) + 1);

            PrintWorld(world,
                coords.X - (radius / 2),
                coords.X + (radius / 2) + 1,
                coords.Y - (radius / 2),
                coords.Y + (radius / 2) + 1);

            Console.WriteLine();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static int AskForInt(string v, int min, int max)
        {
            int value = 0;
            while(true)
            {
                Console.Write($"Enter {v} [{min}->{max}]: ");
                if (int.TryParse(Console.ReadLine(), out int result))
                {
                    if (result >= min && result <= max)
                    {
                        value = result;
                        break;
                    }
                }
            }

            return value;
        }

        private static void ProcessWorld(Land[,] world, int startX, int stopX, int startY, int stopY)
        {
            var lands = new List<Land>();
            for (int y = startY; y < world.GetLength(1) && y < stopY; y++)
            {
                for (int x = startX; x < world.GetLength(0) && x < stopX; x++)
                {
                    if (!world[x, y].IsPlaza)
                    {
                        lands.Add(world[x, y]);
                    }
                }
            }

            Parallel.ForEach(lands, land => GetLandInfo(land));

            // For debugging
            //foreach(var land in lands)
            //{
            //    GetLandInfo(land);
            //}
        }

        private static void PrintWorld(World world, int startX, int stopX, int startY, int stopY)
        {
            Table table = new Table();

            table.AddColumn("");
            for (int x = startX; x < world.Lands.GetLength(0) && x < stopX; x++)
            {
                table.AddColumn(world.GetXCoordinate(x).ToString());
            }

            for (int y = startY; y < world.Lands.GetLength(1) && y < stopY; y++)
            {
                var rowCellData = new List<IRenderable>();
                rowCellData.Add(new Markup(world.GetYCoordinate(y).ToString()));

                for (int x = startX; x < world.Lands.GetLength(0) && x < stopX; x++)
                {
                    if (world.Lands[x, y]?.IsPlaza ?? false)
                    {
                        rowCellData.Add(new Panel("PLAZA"));
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        if (world.Lands[x, y]?.ForSale ?? false)
                        {
                            sb.AppendLine($"[green]{world.Lands[x, y].SalesPrice} - {world.Lands[x, y].SaleListedTimestamp}[/]");
                        }

                        if (world.Lands[x, y]?.RecentlySold ?? false)
                        {                            
                            foreach(var rsp in world.Lands[x, y].RecentlySoldPrices)
                            {
                                sb.AppendLine($"[red]{rsp.ToString()}[/]");
                            }                            
                        }

                        if (sb.Length > 0)
                        {
                            rowCellData.Add(new Panel(sb.ToString()));
                        }
                        else
                        {
                            rowCellData.Add(new Markup(""));
                        }
                    }
                }

                table.AddRow(rowCellData);
                table.AddEmptyRow();
            }

            AnsiConsole.Write(table);
        }

        private static void GetLandInfo(Land land)
        {
            GetCnftInfo(land);
            GetCnftAnalyticsInfo(land);
        }

        private static void GetCnftAnalyticsInfo(Land land)
        {
            var client = new RestClient($"https://cnftanalytics.io/fphp/search_results.php/?search=pavia^$^{land.X}%20{land.Y}&filter=1&page=1&offset=-120");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "nl-NL,nl;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("Referer", "https://cnftanalytics.io/php/legacySearch.php");
            request.AddHeader("Sec-Fetch-Dest", "empty");
            request.AddHeader("Sec-Fetch-Mode", "cors");
            request.AddHeader("Sec-Fetch-Site", "same-origin");
            request.AddHeader("Sec-GPC", "1");
            request.AddHeader("Cookie", "Theme=dark");
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                var content = response.Content;

                // fix closing tag
                if (content.EndsWith("</"))
                {
                    content += "tr>";
                }

                // fix missing space
                if (content.Contains("\"target"))
                {
                    content = content.Replace("\"target", "\" target");
                }

                // enclose in root node
                content = "<root>" + content + "</root>";

                System.Xml.XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);

                // foreach tr
                var trNodes = doc.DocumentElement.SelectNodes("tr");

                var soldPricesList = new List<SoldForPrice>();
                foreach (XmlNode trNode in trNodes)
                {
                    var tdNodes = trNode.SelectNodes("td");
                    var soldDate = tdNodes[1].InnerText;
                    var price = tdNodes[2].InnerText;
                    if (decimal.TryParse(price, out decimal parsedPrice))
                    {
                        SoldForPrice sfp = new SoldForPrice
                        {
                            Price = parsedPrice,
                            Timestamp = soldDate
                        };

                        soldPricesList.Add(sfp);
                    }
                }

                land.RecentlySoldPrices.AddRange(soldPricesList);
            }
        }

        private static void GetCnftInfo(Land land)
        {
            // CNFT.io
            var client = new RestClient("https://api.cnft.io/market/listings");
            client.Timeout = -1;
            client.UseNewtonsoftJson();
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            var sb = new StringBuilder();
            sb.Append("pavia");
            if (land.X >= 0)
            {
                sb.Append("plus");
            }
            else
            {
                sb.Append("minus");
            }
            sb.Append(Math.Abs(land.X));

            if (land.Y >= 0)
            {
                sb.Append("plus");
            }
            else
            {
                sb.Append("minus");
            }
            sb.Append(Math.Abs(land.Y));

            request.AddJsonBody(new MarketListingFilter
            {
                Search = sb.ToString(),
                Verified = true,
                Page = 1,
                Nsfw = false,
                Sold = false,
                Sort = new Sort {  Id = -1 },
                Types = new[] {"listing", "auction", "offer" }
            });

            IRestResponse response = client.Execute(request);

            var result = JsonConvert.DeserializeObject<MarketplaceSearchResult>(response.Content);

            if (result.Results != null && result.Results.Count() == 1)
            {
                var resultFound = result.Results.FirstOrDefault();

                land.ForSale = true;
                land.SalesPrice = resultFound.Price / 1000000;

                if (resultFound.CreatedAt != null)
                {
                    land.SaleListedTimestamp = resultFound.CreatedAt.ToString("dd/MM HH:mm");
                }

                if (resultFound.UpdatedAt != null)
                {
                    land.SaleListedTimestamp = resultFound.UpdatedAt.ToString("dd/MM HH:mm");
                }
            }
        }
    }
}
