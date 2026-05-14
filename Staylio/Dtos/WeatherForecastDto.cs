using Newtonsoft.Json;

namespace Staylio.Dtos
{
    public class WeatherForecastDto
    {
        public string cod { get; set; }
        public int message { get; set; }
        public int cnt { get; set; }
        public List<WeatherForecastItemDto> list { get; set; }
        public WeatherCityDto city { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class WeatherForecastItemDto
    {
        public long dt { get; set; }

        [JsonProperty("summery")]
        public string Summary { get; set; }

        public WeatherMainDto main { get; set; }
        public List<WeatherInfoDto> weather { get; set; }
        public WeatherCloudDto clouds { get; set; }
        public WeatherWindDto wind { get; set; }
        public int visibility_distance { get; set; }
        public string visibility_unit { get; set; }
        public int probability_of_precipitation { get; set; }
        public string probability_of_precipitation_unit { get; set; }
        public WeatherRainSnowDto rain { get; set; }
        public WeatherRainSnowDto snow { get; set; }
        public string dt_txt { get; set; }
    }

    public class WeatherMainDto
    {
        [JsonProperty("temprature")]
        public double Temperature { get; set; }

        [JsonProperty("temprature_feels_like")]
        public double TemperatureFeelsLike { get; set; }

        [JsonProperty("temprature_min")]
        public double TemperatureMin { get; set; }

        [JsonProperty("temprature_max")]
        public double TemperatureMax { get; set; }

        [JsonProperty("temprature_unit")]
        public string TemperatureUnit { get; set; }

        public int pressure { get; set; }
        public string pressure_unit { get; set; }
        public int humidity { get; set; }
        public string humidity_unit { get; set; }
    }

    public class WeatherInfoDto
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class WeatherCloudDto
    {
        public int cloudiness { get; set; }
        public string unit { get; set; }
    }

    public class WeatherWindDto
    {
        public double speed { get; set; }
        public int degrees { get; set; }
        public string direction { get; set; }
        public double gust_speed { get; set; }
        public string speed_unit { get; set; }
    }

    public class WeatherRainSnowDto
    {
        public double amount { get; set; }
        public string unit { get; set; }
    }

    public class WeatherCityDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public WeatherCoordinateDto coord { get; set; }
        public string country { get; set; }
        public int population { get; set; }
        public int timezone { get; set; }
        public long sunrise { get; set; }
        public string sunrise_txt { get; set; }
        public long sunset { get; set; }
        public string sunset_txt { get; set; }
    }

    public class WeatherCoordinateDto
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }
}
