using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Staylio.Dtos;
using Staylio.Models;

namespace Staylio.Controllers
{
    public class BookingController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _apiHost = "booking-com15.p.rapidapi.com";

        public BookingController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = configuration["RapidApi:Key"] ?? throw new InvalidOperationException("API Key not configured");
        }

        [HttpGet]
        public IActionResult Search()
        {
            var model = new SearchViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> HotelList(SearchViewModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.City))
            {
                ModelState.AddModelError("City", "Şehir adı boş olamaz");
                return View("Search", searchModel);
            }

            try
            {
                // ✅ ADIM 1: Lokasyon Ara
                var locationRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{_apiHost}/api/v1/hotels/searchDestination?query={Uri.EscapeDataString(searchModel.City)}"),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", _apiHost }
                    }
                };

                var locationResponse = await _httpClient.SendAsync(locationRequest);
                var locationBody = await locationResponse.Content.ReadAsStringAsync();

                if (!locationResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Şehir bulunamadı. Lütfen başka bir şehir deneyin.";
                    return RedirectToAction("Search");
                }

                var locationData = JsonConvert.DeserializeObject<BookingDestinationResponseDto>(locationBody);

                if (locationData?.data == null || locationData.data.Count == 0)
                {
                    TempData["ErrorMessage"] = $"'{searchModel.City}' şehri bulunamadı.";
                    return RedirectToAction("Search");
                }

                var selectedLocation = locationData.data.FirstOrDefault();

                // ✅ ADIM 2: Tarihleri Hazırla
                var checkin = searchModel.CheckInDate.ToString("yyyy-MM-dd");
                var checkout = searchModel.CheckOutDate.ToString("yyyy-MM-dd");

                // ✅ ADIM 3: Otel Ara
                var hotelRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://{_apiHost}/api/v1/hotels/searchHotels" +
                        $"?dest_id={selectedLocation.dest_id}" +
                        $"&search_type={selectedLocation.search_type}" +
                        $"&arrival_date={checkin}" +
                        $"&departure_date={checkout}" +
                        $"&adults={searchModel.Guests}" +
                        $"&room_qty=1" +
                        $"&currency_code=TRY" +
                        $"&languagecode=tr"
                    ),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", _apiHost }
                    }
                };

                var hotelResponse = await _httpClient.SendAsync(hotelRequest);
                var hotelBody = await hotelResponse.Content.ReadAsStringAsync();

                if (!hotelResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Otel listesi alınamadı. Lütfen tekrar deneyin.";
                    return RedirectToAction("Search");
                }

                var hotels = JsonConvert.DeserializeObject<BookingHotelSearchResponseDto>(hotelBody);

                if (hotels?.data?.hotels == null || hotels.data.hotels.Count == 0)
                {
                    TempData["ErrorMessage"] = "Bu kriterlere uygun otel bulunamadı.";
                    TempData["SearchCity"] = searchModel.City;
                    TempData["CheckInDate"] = checkin;
                    TempData["CheckOutDate"] = checkout;
                    TempData["Guests"] = searchModel.Guests;
                    return View(new List<BookingHotelDto>());
                }

                // ✅ Arama bilgilerini saklı tutacağız
                TempData["SearchCity"] = searchModel.City;
                TempData["CheckInDate"] = checkin;
                TempData["CheckOutDate"] = checkout;
                TempData["Guests"] = searchModel.Guests;

                return View(hotels.data.hotels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return RedirectToAction("Search");
            }
        }

        [HttpGet]
        public async Task<IActionResult> HotelDetail(int id)
        {
            try
            {
                // Tarihler TempData'dan gelecek veya default değerler
                var checkinStr = TempData["CheckInDate"]?.ToString() ?? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                var checkoutStr = TempData["CheckOutDate"]?.ToString() ?? DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");

                // TempData'yı tekrar set et (sayfada gösterilmesi için)
                TempData["CheckInDate"] = checkinStr;
                TempData["CheckOutDate"] = checkoutStr;

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://{_apiHost}/api/v1/hotels/getHotelDetails" +
                        $"?hotel_id={id}" +
                        $"&arrival_date={checkinStr}" +
                        $"&departure_date={checkoutStr}" +
                        $"&adults=2" +
                        $"&children_age=0" +
                        $"&room_qty=1" +
                        $"&units=metric" +
                        $"&temperature_unit=c" +
                        $"&languagecode=tr" +
                        $"&currency_code=TRY"
                    ),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", _apiHost }
                    }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Otel detayları yüklenemedi.";
                    return RedirectToAction("HotelList");
                }

                var hotelResponse = JsonConvert.DeserializeObject<HotelDetailResponseDto>(body);

                if (hotelResponse?.data == null)
                {
                    TempData["ErrorMessage"] = "Otel bilgileri boş.";
                    return RedirectToAction("HotelList");
                }

                var photoRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{_apiHost}/api/v1/hotels/getHotelPhotos?hotel_id={id}"),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", _apiHost }
                    }
                };

                var photoResponse = await _httpClient.SendAsync(photoRequest);
                var photoBody = await photoResponse.Content.ReadAsStringAsync();

                if (photoResponse.IsSuccessStatusCode)
                {
                    var photos = JsonConvert.DeserializeObject<HotelPhotoResponseDto>(photoBody);
                    ViewBag.Photos = photos?.data ?? new List<HotelPhotoDto>();
                }
                else
                {
                    ViewBag.Photos = new List<HotelPhotoDto>();
                }

                return View(hotelResponse.data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return RedirectToAction("HotelList");
            }
        }
        public async Task<IActionResult> Weather()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://weather-api167.p.rapidapi.com/api/weather/forecast?place=London%2CGB&cnt=3&units=metric&type=three_hour&mode=json&lang=en"),
                Headers =
        {
            { "x-rapidapi-key", _apiKey },
            { "x-rapidapi-host", "weather-api167.p.rapidapi.com" },
            { "Accept", "application/json" },
        },
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var weather = JsonConvert.DeserializeObject<WeatherForecastDto>(body);

            return View(weather);
        }

        /// <summary>
        /// Şehir arama için autocomplete (isteğe bağlı)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchCities(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<BookingDestinationDto>());
            }

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{_apiHost}/api/v1/hotels/searchDestination?query={Uri.EscapeDataString(query)}"),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", _apiHost }
                    }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new List<BookingDestinationDto>());
                }

                var destinations = JsonConvert.DeserializeObject<BookingDestinationResponseDto>(body);
                return Json(destinations?.data?.Take(10).ToList() ?? new List<BookingDestinationDto>());
            }
            catch
            {
                return Json(new List<BookingDestinationDto>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCrypto()
        {
            var cryptoHost = "coingecko.p.rapidapi.com";
            CryptoPriceResponseDto crypto = null;
            var source = "CoinGecko RapidAPI";

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://{cryptoHost}/simple/price" +
                        "?ids=bitcoin%2Cethereum" +
                        "&vs_currencies=usd%2Ctry"
                    ),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", cryptoHost },
                        { "Accept", "application/json" },
                        { "User-Agent", "StaylioHotelSearch/1.0" }
                    }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    crypto = JsonConvert.DeserializeObject<CryptoPriceResponseDto>(body);
                }
            }
            catch
            {
                crypto = null;
            }

            if (crypto?.bitcoin == null || crypto.ethereum == null)
            {
                try
                {
                    var fallbackRequest = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("https://api.coingecko.com/api/v3/simple/price?ids=bitcoin%2Cethereum&vs_currencies=usd%2Ctry"),
                        Headers =
                        {
                            { "Accept", "application/json" },
                            { "User-Agent", "StaylioHotelSearch/1.0" }
                        }
                    };

                    var fallbackResponse = await _httpClient.SendAsync(fallbackRequest);
                    var fallbackBody = await fallbackResponse.Content.ReadAsStringAsync();

                    if (fallbackResponse.IsSuccessStatusCode)
                    {
                        crypto = JsonConvert.DeserializeObject<CryptoPriceResponseDto>(fallbackBody);
                        source = "CoinGecko public API";
                    }
                }
                catch
                {
                    crypto = null;
                }
            }

            if (crypto?.bitcoin == null || crypto.ethereum == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kripto fiyatları alınamadı. RapidAPI planı veya CoinGecko erişimi engelliyor olabilir."
                });
            }

            return Json(new
            {
                success = true,
                source,
                bitcoinTry = crypto.bitcoin.Try.ToString("N0"),
                bitcoinUsd = crypto.bitcoin.usd.ToString("N0"),
                ethereumTry = crypto.ethereum.Try.ToString("N0"),
                ethereumUsd = crypto.ethereum.usd.ToString("N0")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrency()
        {
            var currencyHost = "currency-conversion-and-exchange-rates.p.rapidapi.com";

            try
            {
                var usdEur = await ConvertCurrencyAsync(currencyHost, "USD", "EUR", 1);
                var usdTry = await ConvertCurrencyAsync(currencyHost, "USD", "TRY", 1);
                var eurTry = await ConvertCurrencyAsync(currencyHost, "EUR", "TRY", 1);

                if (!usdEur.HasValue || !usdTry.HasValue || !eurTry.HasValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Döviz verisi boş geldi."
                    });
                }

                return Json(new
                {
                    success = true,
                    usdEur = usdEur.Value.ToString("N4"),
                    usdTry = usdTry.Value.ToString("N2"),
                    eurTry = eurTry.Value.ToString("N2")
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Döviz servisine ulaşılamadı."
                });
            }
        }

        private async Task<decimal?> ConvertCurrencyAsync(string host, string from, string to, decimal amount)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://{host}/convert" +
                    $"?from={Uri.EscapeDataString(from)}" +
                    $"&to={Uri.EscapeDataString(to)}" +
                    $"&amount={amount.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                ),
                Headers =
                {
                    { "x-rapidapi-key", _apiKey },
                    { "x-rapidapi-host", host },
                    { "Accept", "application/json" }
                }
            };

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var root = JToken.Parse(body);
            var result = ReadDecimalValue(root["result"]);

            if (result.HasValue)
            {
                return result.Value / amount;
            }

            var rate = ReadDecimalValue(root["info"]?["rate"]);
            return rate;
        }

        private static decimal? ReadDecimalValue(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            var rawValue = token.ToString().Trim();

            if (rawValue.Contains(",") && decimal.TryParse(rawValue, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("tr-TR"), out var value))
            {
                return value;
            }

            if (decimal.TryParse(rawValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            var normalizedValue = rawValue.Replace(".", string.Empty).Replace(",", ".");

            if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetGasoline(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return Json(new
                {
                    success = false,
                    message = "Akaryakıt fiyatını görmek için önce şehir seçin."
                });
            }

            var gasHost = "gas-price.p.rapidapi.com";

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{gasHost}/europeanCountries"),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", gasHost },
                        { "Accept", "application/json" }
                    }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Akaryakıt fiyatları alınamadı."
                    });
                }

                var root = JToken.Parse(body);
                var fuelItems = ExtractFuelItems(root)
                    .Select(item => new FuelPriceViewItem
                    {
                        country = ReadTokenValue(item, "country", "countryName", "name", "ulke", "city") ?? "Avrupa",
                        gasoline = ReadTokenValue(item, "gasoline", "gasolinePrice", "gasoline_price", "benzin", "e5", "e10", "unleaded") ?? "-",
                        diesel = ReadTokenValue(item, "diesel", "dieselPrice", "diesel_price", "motorin") ?? "-",
                        lpg = ReadTokenValue(item, "lpg", "lpgPrice", "lpg_price") ?? "-",
                        currency = ReadTokenValue(item, "currency", "currencyCode", "unit") ?? "EUR"
                    })
                    .ToList();

                var selectedCountry = ResolveFuelCountry(city);
                var selectedFuelItems = fuelItems
                    .Where(item => FuelTextMatches(item.country, selectedCountry) || FuelTextMatches(item.country, city))
                    .Take(4)
                    .ToList();

                if (selectedFuelItems.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"{city} için akaryakıt verisi bulunamadı. Bu endpoint şehirden çok ülke bazlı çalışıyor."
                    });
                }

                return Json(new
                {
                    success = true,
                    selectedCity = city,
                    selectedCountry,
                    items = selectedFuelItems
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Akaryakıt servisine ulaşılamadı."
                });
            }
        }

        private class FuelPriceViewItem
        {
            public string country { get; set; }
            public string gasoline { get; set; }
            public string diesel { get; set; }
            public string lpg { get; set; }
            public string currency { get; set; }
        }

        private static string ResolveFuelCountry(string city)
        {
            var normalizedCity = NormalizeFuelText(city);

            if (normalizedCity.Contains("istanbul") || normalizedCity.Contains("ankara") || normalizedCity.Contains("izmir"))
            {
                return "Turkey";
            }

            if (normalizedCity.Contains("london"))
            {
                return "United Kingdom";
            }

            if (normalizedCity.Contains("paris"))
            {
                return "France";
            }

            if (normalizedCity.Contains("berlin"))
            {
                return "Germany";
            }

            if (normalizedCity.Contains("rome") || normalizedCity.Contains("roma"))
            {
                return "Italy";
            }

            if (normalizedCity.Contains("madrid"))
            {
                return "Spain";
            }

            return city;
        }

        private static bool FuelTextMatches(string source, string query)
        {
            var normalizedSource = NormalizeFuelText(source);
            var normalizedQuery = NormalizeFuelText(query);

            return !string.IsNullOrWhiteSpace(normalizedSource)
                && !string.IsNullOrWhiteSpace(normalizedQuery)
                && (normalizedSource.Contains(normalizedQuery) || normalizedQuery.Contains(normalizedSource));
        }

        private static string NormalizeFuelText(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace("turkiye", "turkey")
                .Replace("united kingdom", "uk");
        }

        private static IEnumerable<JToken> ExtractFuelItems(JToken root)
        {
            var token = root.Type == JTokenType.Array
                ? root
                : root["result"] ?? root["data"] ?? root["prices"] ?? root["countries"] ?? root;

            if (token.Type == JTokenType.Array)
            {
                return token.Children();
            }

            if (token.Type != JTokenType.Object)
            {
                return Enumerable.Empty<JToken>();
            }

            var obj = (JObject)token;
            var arrayProperty = obj.Properties().FirstOrDefault(property => property.Value.Type == JTokenType.Array);

            if (arrayProperty != null)
            {
                return arrayProperty.Value.Children();
            }

            return obj.Properties()
                .Where(property => property.Value.Type == JTokenType.Object)
                .Select(property =>
                {
                    var clone = (JObject)property.Value.DeepClone();

                    if (clone["country"] == null && clone["name"] == null)
                    {
                        clone["country"] = property.Name;
                    }

                    return clone;
                });
        }

        private static string ReadTokenValue(JToken token, params string[] names)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;

                foreach (var property in obj.Properties())
                {
                    var isMatch = names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase));

                    if (isMatch && property.Value.Type != JTokenType.Object && property.Value.Type != JTokenType.Array)
                    {
                        return property.Value.ToString();
                    }
                }

                foreach (var property in obj.Properties())
                {
                    var nestedValue = ReadTokenValue(property.Value, names);

                    if (!string.IsNullOrWhiteSpace(nestedValue))
                    {
                        return nestedValue;
                    }
                }
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    var nestedValue = ReadTokenValue(child, names);

                    if (!string.IsNullOrWhiteSpace(nestedValue))
                    {
                        return nestedValue;
                    }
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return Json(new
                {
                    success = false,
                    message = "Hava durumunu görmek için önce şehir seçin."
                });
            }

            var weatherPlace = ResolveWeatherPlace(city);
            var weather = await GetWeatherForecastAsync(weatherPlace);

            if (!weather.IsSuccess && weatherPlace != city)
            {
                weather = await GetWeatherForecastAsync(city);
            }

            var currentWeather = weather.list?.FirstOrDefault();
            var weatherInfo = currentWeather?.weather?.FirstOrDefault();

            if (!weather.IsSuccess || currentWeather == null)
            {
                return Json(new
                {
                    success = false,
                    message = weather.ErrorMessage ?? "Hava durumu bilgisi alınamadı."
                });
            }

            return Json(new
            {
                success = true,
                city = weather.city != null ? $"{weather.city.name}, {weather.city.country}" : city,
                condition = weatherInfo?.main ?? "Weather",
                description = weatherInfo?.description ?? "Tahmin",
                icon = weatherInfo?.icon,
                temperature = currentWeather.main?.Temperature.ToString("F0") ?? "-",
                feelsLike = currentWeather.main?.TemperatureFeelsLike.ToString("F0") ?? "-",
                unit = currentWeather.main?.TemperatureUnit == "K" ? "K" : "°C",
                humidity = currentWeather.main?.humidity.ToString() ?? "-",
                wind = currentWeather.wind != null
                    ? $"{currentWeather.wind.speed:F1} {currentWeather.wind.speed_unit}"
                    : "-",
                date = currentWeather.dt_txt
            });
        }

        private async Task<WeatherForecastDto> GetWeatherForecastAsync(string place)
        {
            var weatherHost = "weather-api167.p.rapidapi.com";

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://{weatherHost}/api/weather/forecast" +
                        $"?place={Uri.EscapeDataString(place)}" +
                        $"&cnt=3" +
                        $"&units=metric" +
                        $"&type=three_hour" +
                        $"&mode=json" +
                        $"&lang=en"
                    ),
                    Headers =
                    {
                        { "x-rapidapi-key", _apiKey },
                        { "x-rapidapi-host", weatherHost },
                        { "Accept", "application/json" },
                    },
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new WeatherForecastDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "Hava durumu bilgisi alınamadı. " + GetApiErrorMessage(body)
                    };
                }

                var weather = JsonConvert.DeserializeObject<WeatherForecastDto>(body) ?? new WeatherForecastDto();
                weather.IsSuccess = weather.list != null && weather.list.Count > 0;

                if (!weather.IsSuccess)
                {
                    weather.ErrorMessage = "Hava durumu verisi boş geldi.";
                }

                return weather;
            }
            catch
            {
                return new WeatherForecastDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Hava durumu servisine ulaşılamadı."
                };
            }
        }

        private static string ResolveWeatherPlace(string city)
        {
            var normalizedCity = NormalizeFuelText(city);

            if (normalizedCity.Contains("ankara"))
            {
                return "Ankara,TR";
            }

            if (normalizedCity.Contains("istanbul"))
            {
                return "Istanbul,TR";
            }

            if (normalizedCity.Contains("izmir"))
            {
                return "Izmir,TR";
            }

            if (normalizedCity.Contains("london"))
            {
                return "London,GB";
            }

            if (normalizedCity.Contains("paris"))
            {
                return "Paris,FR";
            }

            return city;
        }

        private static string GetApiErrorMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            try
            {
                var root = JToken.Parse(body);
                var message = ReadTokenValue(root, "message", "error", "error_message", "detail");

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message.Length > 120 ? message.Substring(0, 120) : message;
                }
            }
            catch
            {
                // API bazen JSON yerine düz metin döndürebiliyor.
            }

            return body.Length > 120 ? body.Substring(0, 120) : body;
        }
    }
}
