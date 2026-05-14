using Newtonsoft.Json;
using System.Collections.Generic;

namespace Staylio.Dtos
{
    public class HotelDetailResponseDto
    {
        public bool status { get; set; }
        public object message { get; set; }
        public HotelDetailDto data { get; set; }
    }

    public class HotelDetailDto
    {
        [JsonProperty("hotel_id")]
        public string HotelId { get; set; }

        [JsonProperty("hotel_name")]
        public string HotelName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country_trans")]
        public string CountryTrans { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("review_score")]
        public decimal? ReviewScore { get; set; }

        [JsonProperty("review_score_word")]
        public string ReviewScoreWord { get; set; }

        [JsonProperty("accommodation_type_name")]
        public string AccommodationTypeName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("photo_urls")]
        public List<string> PhotoUrls { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("top_ufi_benefits")]
        public List<HotelTopBenefitDto> TopUfiBenefits { get; set; }

        [JsonProperty("facilities_block")]
        public HotelFacilitiesBlockDto FacilitiesBlock { get; set; }
    }

    public class HotelTopBenefitDto
    {
        [JsonProperty("translated_name")]
        public string TranslatedName { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class HotelFacilitiesBlockDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("facilities")]
        public List<HotelFacilityDto> Facilities { get; set; }
    }

    public class HotelFacilityDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}
