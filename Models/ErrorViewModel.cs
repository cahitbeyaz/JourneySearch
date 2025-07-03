namespace ObiletJourneySearch.Models;

/// <summary>
/// Error information displayed to the user.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
