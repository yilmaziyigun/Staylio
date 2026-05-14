namespace Staylio.Dtos
{
    public class HotelPhotoResponseDto
    {
        public bool status { get; set; }
        public object message { get; set; }
        public long timestamp { get; set; }
        public List<HotelPhotoDto> data { get; set; }
    }

    public class HotelPhotoDto
    {
        public int id { get; set; }
        public string url { get; set; }
    }

}
