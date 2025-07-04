using Microsoft.AspNetCore.Mvc;
using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models;
using ObiletJourneySearch.Models.DTOs;
using ObiletJourneySearch.Models.ViewModels;
using ObiletJourneySearch.Utilities;

namespace ObiletJourneySearch.Controllers
{
   
    public class JourneyController : Controller
    {
        private readonly IObiletApiClient _apiClient;
        private readonly ILogger<JourneyController> _logger;

        public JourneyController(
            IObiletApiClient apiClient,
            ILogger<JourneyController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int originId, int destinationId, string originName, string destinationName, string Date)
        {
            try
            {
                if (originId <= 0 || destinationId <= 0)
                {
                    _logger.LogWarning("Invalid location IDs provided: Origin={OriginId}, Destination={DestinationId}", originId, destinationId);
                    return RedirectToAction("Index", "Home", new { errorMessage = "Invalid origin or destination selected." });
                }

                if (originId == destinationId)
                {
                    _logger.LogWarning("Origin and destination are the same: {LocationId}", originId);
                    return RedirectToAction("Index", "Home", new { errorMessage = "Origin and destination cannot be the same location." });
                }

                var session = HttpContext.GetObiletSession();
                if (session == null)
                {
                    _logger.LogError("Failed to retrieve session for journey search");
                    return View("Error", new ErrorViewModel
                    {
                        RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                        ErrorMessage = "Failed to create session with the server. Please try again later."
                    });
                }

                // Use location names passed from the form
                // If names are not provided, use empty strings (should not happen with proper form submission)
                originName = string.IsNullOrEmpty(originName) ? "Unknown Location" : originName;
                destinationName = string.IsNullOrEmpty(destinationName) ? "Unknown Location" : destinationName;

                if (!DateTime.TryParse(Date, out var parsedDepartureDate))
                {
                    _logger.LogWarning("Invalid departure date format: {DepartureDate}. Defaulting to tomorrow.", Date);
                    parsedDepartureDate = DateTime.Now.AddDays(1).Date;
                }

                if (parsedDepartureDate.Date < DateTime.Now.Date)
                {
                    _logger.LogWarning("Past departure date provided: {DepartureDate}", parsedDepartureDate);
                    parsedDepartureDate = DateTime.Now.Date;
                    ViewBag.WarningMessage = "The selected date was in the past. Today's date has been used instead.";
                }

                var model = new JourneyViewModel
                {
                    OriginId = originId,
                    DestinationId = destinationId,
                    OriginName = originName,
                    DestinationName = destinationName,
                    DepartureDate = parsedDepartureDate
                };

                try
                {
                    var journeyRequest = new JourneyRequest
                    {
                        DeviceSession = session,
                        Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Language = "tr-TR",
                        Data = new JourneyRequestData
                        {
                            OriginId = originId,
                            DestinationId = destinationId,
                            DepartureDate = parsedDepartureDate.ToString("yyyy-MM-dd")
                        }
                    };

                    _logger.LogInformation("Requesting journeys: {Origin} to {Destination} on {Date}",
                        originName, destinationName, parsedDepartureDate.ToString("yyyy-MM-dd"));

                    var response = await _apiClient.GetBusJourneysAsync(journeyRequest);

                    if (response.Status == "Success" && response.Data != null)
                    {
                        model.Journeys = response.Data
                            .Where(j => j.IsActive)
                            .OrderBy(j => j.JourneyDetail.Departure)
                            .ToList();

                        _logger.LogInformation("Found {Count} journeys for {Origin} to {Destination}",
                            model.Journeys.Count, originName, destinationName);
                    }
                    else
                    {
                        _logger.LogError("Failed to get journeys. Status: {Status}, Message: {Message}",
                            response.Status, response.Message ?? "No message");

                        ViewBag.ErrorMessage = $"Failed to retrieve journeys: {response.UserMessage ?? response.Message ?? "Unknown error"}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting journeys for {Origin} to {Destination}", originName, destinationName);
                    ViewBag.ErrorMessage = "An error occurred while retrieving journeys. Please try again later.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Journey Index action");
                return View("Error", new ErrorViewModel
                {
                    RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = "An unexpected error occurred. Please try again later."
                });
            }
        }
    }
}
