namespace ObiletJourneySearch.Models;


public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; } = 500; // Default to 500 Internal Server Error
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
