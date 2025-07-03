using System.Text.Json.Serialization;

namespace ObiletJourneySearch.Models.DTOs
{
    public class SessionRequest
    {
        [JsonPropertyName("type")]
        public int Type { get; set; } = 7; // Device type specific for distribusion api client

        [JsonPropertyName("connection")]
        public Connection Connection { get; set; }

        [JsonPropertyName("browser")]
        public Browser Browser { get; set; }

        [JsonPropertyName("application")]
        public Application Application { get; set; }
    }

    public class Connection
    {
        [JsonPropertyName("ip-address")]
        public string IpAddress { get; set; }
        
        [JsonPropertyName("port")]
        public string Port { get; set; }
    }

    public class Browser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public class Application
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0.0";

        [JsonPropertyName("equipment-id")]
        public string EquipmentId { get; set; } = "distribusion";
    }

    public class SessionResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public SessionData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("user-message")]
        public string UserMessage { get; set; }

        [JsonPropertyName("api-request-id")]
        public string ApiRequestId { get; set; }

        [JsonPropertyName("controller")]
        public string Controller { get; set; }
    }

    public class SessionData
    {
        [JsonPropertyName("session-id")]
        public string SessionId { get; set; }

        [JsonPropertyName("device-id")]
        public string DeviceId { get; set; }
    }

    public class DeviceSession
    {
        [JsonPropertyName("session-id")]
        public string SessionId { get; set; }

        [JsonPropertyName("device-id")]
        public string DeviceId { get; set; }
    }
}
