using System;
using Staylio.Dtos;

namespace Staylio.Models
{
    public class SearchViewModel
    {
        public string City { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Guests { get; set; }
        public WeatherForecastDto WeatherForecast { get; set; }

        public SearchViewModel()
        {
            CheckInDate = DateTime.Now.AddDays(1);
            CheckOutDate = DateTime.Now.AddDays(3);
            Guests = 2;
        }
    }
}
