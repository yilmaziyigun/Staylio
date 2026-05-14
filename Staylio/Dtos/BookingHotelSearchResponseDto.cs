namespace Staylio.Dtos
{
    public class BookingHotelSearchResponseDto
    {
        public bool status { get; set; }
        public object message { get; set; }
        public BookingHotelSearchDataDto data { get; set; }
    }

    public class BookingHotelSearchDataDto
    {
        public List<BookingHotelDto> hotels { get; set; }
    }

    public class BookingHotelDto
    {
        public BookingHotelPropertyDto property { get; set; }
        public BookingHotelPriceBreakdownDto priceBreakdown { get; set; }
    }

    public class BookingHotelPropertyDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string wishlistName { get; set; }
        public string address { get; set; }
        public double? reviewScore { get; set; }
        public string reviewScoreWord { get; set; }
        public List<string> photoUrls { get; set; }
    }

    public class BookingHotelPriceBreakdownDto
    {
        public BookingHotelGrossPriceDto grossPrice { get; set; }
    }

    public class BookingHotelGrossPriceDto
    {
        public decimal? value { get; set; }
        public string currency { get; set; }
    }
}