namespace ObiletJourneySearch.Models;

/// <summary>
/// Error information displayed to the user.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the request ID for tracking purposes.
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Gets or sets the specific error message to display.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the HTTP status code for the error.
    /// </summary>
    public int StatusCode { get; set; } = 500; // Default to 500 Internal Server Error
    
    /// <summary>
    /// Determines whether to show the request ID on the error page.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
