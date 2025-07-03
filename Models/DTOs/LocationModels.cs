using System.Text.Json.Serialization;

namespace ObiletJourneySearch.Models.DTOs
{
    public class BusLocationRequest
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("device-session")]
        public DeviceSession DeviceSession { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en-EN";
    }

    public class BusLocationResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public List<BusLocation> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("user-message")]
        public string UserMessage { get; set; }

        [JsonPropertyName("api-request-id")]
        public string ApiRequestId { get; set; }

        [JsonPropertyName("controller")]
        public string Controller { get; set; }
    }

    public class BusLocation
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parent-id")]
        public int? ParentId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("geo-location")]
        public GeoLocation GeoLocation { get; set; }

        [JsonPropertyName("tz-code")]
        public string TzCode { get; set; }

        [JsonPropertyName("weather-code")]
        public string WeatherCode { get; set; }

        [JsonPropertyName("rank")]
        public int? Rank { get; set; }

        [JsonPropertyName("reference-code")]
        public string ReferenceCode { get; set; }

        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }
    }

    public class GeoLocation
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("zoom")]
        public int Zoom { get; set; }
    }
}
