using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models;
using ObiletJourneySearch.Models.DTOs;
using ObiletJourneySearch.Models.ViewModels;
using ObiletJourneySearch.Services.Caching;
using ObiletJourneySearch.Utilities;
using System.Diagnostics;

namespace ObiletJourneySearch.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IObiletApiClient _apiClient;
    private readonly ILocationCacheService _locationCacheService;
    private const string SearchPreferencesCookieKey = "SearchPreferences";

    public HomeController(
        ILogger<HomeController> logger,
        IObiletApiClient apiClient,
        ILocationCacheService locationCacheService)
    {
        _logger = logger;
        _apiClient = apiClient;
        _locationCacheService = locationCacheService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var session = HttpContext.GetObiletSession();
            if (session == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = "Failed to create session with the server. Please try again later." });
            }

            //select2 makes Search request anyways, even if inital data is loaded. Therefore I have commented out below code until a further implementation

            return View();
        }
        catch (Exception)
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = "An unexpected error occurred. Please try again later." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill in all required fields.";
                return View("Index", model);
            }

            if (model.OriginId == model.DestinationId)
            {
                ViewBag.ErrorMessage = "Origin and destination cannot be the same location.";
                return View("Index", model);
            }

            if (model.Date < DateTime.Now.Date)
            {
                ViewBag.ErrorMessage = "Departure date cannot be in the past.";
                return View("Index", model);
            }

            return RedirectToAction("Index", "Journey", new
            {
                originId = model.OriginId,
                destinationId = model.DestinationId,
                date = model.Date.ToString("yyyy-MM-dd")
            });
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "An error occurred while processing your search request.";
            return View("Index", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchBusLocations(string searchTerm)
    {
        try
        {
            var session = HttpContext.GetObiletSession();
            if (session == null)
            {
                return Json(new { success = false, message = "Failed to create session with the server." });
            }

            var locationsRequest = new BusLocationRequest
            {
                Data = searchTerm,
                DeviceSession = session,
                Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Language = "tr-TR"
            };

            var locationsResponse = await _apiClient.GetBusLocationsAsync(locationsRequest);

            if (locationsResponse.Status == "Success" && locationsResponse.Data != null)
            {
                var locations = locationsResponse.Data
                    .OrderBy(l => l.Rank ?? int.MaxValue)
                    .Select(l => new { id = l.Id, text = l.Name })
                    .ToList();

                return Json(new { success = true, results = locations });
            }
            else
            {
                return Json(new { success = false, message = locationsResponse.UserMessage ?? locationsResponse.Message ?? "An error occurred while searching locations." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for origin locations");
            return Json(new { success = false, message = "An error occurred while searching for locations." });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code = null)
    {
        // Use the provided code or default to 500 (Internal Server Error)
        var statusCode = code ?? 500;

        if (statusCode == 0)
        {
            // Default to 500 if no status code is specified
            statusCode = 500;
        }

        Response.StatusCode = statusCode;

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode,
            // ErrorMessage will be set in the view based on status code if not provided
            ErrorMessage = TempData["ErrorMessage"]?.ToString()
        });
    }
}
