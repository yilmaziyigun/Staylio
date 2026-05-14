using Newtonsoft.Json;

namespace Staylio.Dtos
{
    public class CryptoPriceResponseDto
    {
        public CryptoCurrencyPriceDto bitcoin { get; set; }
        public CryptoCurrencyPriceDto ethereum { get; set; }
    }

    public class CryptoCurrencyPriceDto
    {
        public decimal usd { get; set; }

        [JsonProperty("try")]
        public decimal Try { get; set; }
    }
}
