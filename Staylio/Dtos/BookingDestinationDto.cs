namespace Staylio.Dtos
{
    public class BookingDestinationResponseDto
    {
        public bool status { get; set; }
        public object message { get; set; }
        public List<BookingDestinationDto> data { get; set; }
    }

    public class BookingDestinationDto
    {
        public string dest_id { get; set; }
        public string search_type { get; set; }
        public string label { get; set; }
        public string city_name { get; set; }
        public string country { get; set; }
    }
}