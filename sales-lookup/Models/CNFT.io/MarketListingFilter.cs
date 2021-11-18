using Newtonsoft.Json;

namespace sales_lookup.Models.CNFT.io
{
    public partial class MarketListingFilter
    {
        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }

        [JsonProperty("project")]
        public object Project { get; set; }

        [JsonProperty("sort")]
        public Sort Sort { get; set; }

        [JsonProperty("priceMin")]
        public object PriceMin { get; set; }

        [JsonProperty("priceMax")]
        public object PriceMax { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("sold")]
        public bool Sold { get; set; }
    }

    public partial class Sort
    {
        [JsonProperty("_id")]
        public long Id { get; set; }
    }
}
