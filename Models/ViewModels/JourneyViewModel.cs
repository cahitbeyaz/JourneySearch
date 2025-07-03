using ObiletJourneySearch.Models.DTOs;

namespace ObiletJourneySearch.Models.ViewModels
{
    public class JourneyViewModel
    {
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public DateTime DepartureDate { get; set; }
        public List<Journey> Journeys { get; set; } = new List<Journey>();
        public int OriginId { get; set; }
        public int DestinationId { get; set; }
    }
}
