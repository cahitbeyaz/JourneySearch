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

/// <summary>
/// Handles the home page and search form operations including bus locations and session management.
/// </summary>

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

    /// <summary>
    /// Displays the main search form to the user. Loads available bus locations and
    /// previously saved search preferences from cookies if available.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter bus locations</param>
    /// <returns>Search view with populated locations and user preferences</returns>
    public async Task<IActionResult> Index(string searchTerm = null)
    {
        try
        {
            // Get session from HttpContext (managed by middleware)
            var session = HttpContext.GetObiletSession();
            if (session == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = "Failed to create session with the server. Please try again later." });
            }

            var model = new SearchViewModel();

            // Display any TempData messages that might have been set from previous actions
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            // Get saved search preferences from the cookie if they exist
            var savedPreferences = GetSearchPreferencesFromCookie();
            if (savedPreferences != null)
            {
                model.OriginId = savedPreferences.OriginId;
                model.OriginName = savedPreferences.OriginName;
                model.DestinationId = savedPreferences.DestinationId;
                model.DestinationName = savedPreferences.DestinationName;
                model.Date = savedPreferences.Date;
            }
            else
            {
                // Default to tomorrow's date if no saved preferences
                model.Date = DateTime.Now.AddDays(1).Date;
            }

            try
            {
                // Get bus locations from API - when searchTerm is null, API returns initial list
                var locationsRequest = new BusLocationRequest
                {
                    // Pass null data to get initial list of locations when searchTerm is null
                    Data = searchTerm,
                    DeviceSession = session,
                    Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Language = "tr-TR"
                };

                // When searching for specific locations, use API directly (don't cache searches)
                BusLocationResponse locationsResponse;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    _logger.LogInformation("Searching for locations with term: {SearchTerm}", searchTerm);
                    locationsResponse = await _apiClient.GetBusLocationsAsync(locationsRequest);
                }
                else
                {
                    _logger.LogInformation("Getting all locations from cache");
                    locationsResponse = await _locationCacheService.GetAllLocationsAsync(locationsRequest);
                }

                if (locationsResponse.Status == "Success" && locationsResponse.Data != null)
                {
                    // Create select list items for the dropdown
                    model.LocationOptions = locationsResponse.Data
                        .OrderBy(l => l.Rank ?? int.MaxValue)
                        .Select(l => new SelectListItem
                        {
                            Value = l.Id.ToString(),
                            Text = l.Name
                        })
                        .ToList();
                }
                else
                {
                    ViewBag.ErrorMessage = $"Failed to retrieve bus locations: {locationsResponse.Message ?? "Unknown error"}";
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving bus locations. Please try again later.";
            }

            return View(model);
        }
        catch (Exception)
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Processes the search form submission, validates the input, and redirects to the journey results page.
    /// </summary>
    /// <param name="model">Search view model containing user selected origin, destination, and date</param>
    /// <returns>Redirect to journey results page or back to search form with validation errors</returns>
    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Keep the model and re-populate the locations
                ViewBag.ErrorMessage = "Please fill in all required fields.";
                await PopulateLocationsInModel(model);
                return View("Index", model);
            }

            if (model.OriginId == model.DestinationId)
            {
                ViewBag.ErrorMessage = "Origin and destination cannot be the same location.";
                await PopulateLocationsInModel(model);
                return View("Index", model);
            }

            if (model.Date < DateTime.Now.Date)
            {
                ViewBag.ErrorMessage = "Departure date cannot be in the past.";
                await PopulateLocationsInModel(model);
                return View("Index", model);
            }

            // Save search preferences to cookie
            SaveSearchPreferencesToCookie(model);

            // Redirect to the Journey controller's Index action with the search parameters
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
            await PopulateLocationsInModel(model);
            return View("Index", model);
        }
    }

    /// <summary>
    /// Swaps the origin and destination locations in the search form.
    /// </summary>
    /// <param name="model">Search view model containing the current origin and destination</param>
    /// <returns>Index view with swapped locations</returns>
    [HttpPost]
    public IActionResult SwapLocations(SearchViewModel model)
    {
        // Swap origin and destination
        var tempId = model.OriginId;
        var tempName = model.OriginName;

        model.OriginId = model.DestinationId;
        model.OriginName = model.DestinationName;

        model.DestinationId = tempId;
        model.DestinationName = tempName;

        // Update cookie with the new preferences
        SaveSearchPreferencesToCookie(model);

        TempData["SuccessMessage"] = "Origin and destination locations have been swapped.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sets the departure date to either today or tomorrow based on user selection.
    /// </summary>
    /// <param name="dateType">Date selection type ("Today" or "Tomorrow")</param>
    /// <returns>Redirect to Index view with updated date</returns>
    [HttpPost]
    public IActionResult SetDate(string dateType)
    {
        var model = new SearchViewModel();

        // Get saved search preferences from the cookie if they exist
        var savedPreferences = GetSearchPreferencesFromCookie();
        if (savedPreferences != null)
        {
            model = savedPreferences;
        }

        switch (dateType?.ToLower() ?? string.Empty)
        {
            case "today":
                model.Date = DateTime.Now.Date;
                TempData["SuccessMessage"] = "Departure date set to today.";
                break;
            case "tomorrow":
                model.Date = DateTime.Now.Date.AddDays(1);
                TempData["SuccessMessage"] = "Departure date set to tomorrow.";
                break;
            default:
                TempData["ErrorMessage"] = "Invalid date selection.";
                break;
        }

        SaveSearchPreferencesToCookie(model);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
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

    /// <summary>
    /// API endpoint to search for bus locations by text for origin field
    /// </summary>
    /// <param name="searchTerm">Text to search for</param>
    /// <returns>JSON result with matching locations</returns>
    [HttpGet]
    public async Task<IActionResult> SearchOriginLocations(string searchTerm)
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
                Language = "en-EN"
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

    /// <summary>
    /// API endpoint to search for bus locations by text for destination field
    /// </summary>
    /// <param name="searchTerm">Text to search for</param>
    /// <returns>JSON result with matching locations</returns>
    [HttpGet]
    public async Task<IActionResult> SearchDestinationLocations(string searchTerm)
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
                Language = "en-EN"
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
            _logger.LogError(ex, "Error searching for destination locations");
            return Json(new { success = false, message = "An error occurred while searching for locations." });
        }
    }

    /// <summary>
    /// Helper method to populate location options in the search view model
    /// </summary>
    private async Task PopulateLocationsInModel(SearchViewModel model)
    {
        // Get session from HttpContext (managed by middleware)
        var session = HttpContext.GetObiletSession();
        if (session == null)
        {
            return;
        }

        var locationsRequest = new BusLocationRequest
        {
            DeviceSession = session,
            Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Language = "en-EN"
        };

        // Use location cache service instead of directly calling the API
        _logger.LogInformation("Attempting to get locations from cache");
        var locationsResponse = await _locationCacheService.GetAllLocationsAsync(locationsRequest);

        if (locationsResponse.Status == "Success" && locationsResponse.Data != null)
        {
            // Create select list items for the dropdown
            model.LocationOptions = locationsResponse.Data
                .OrderBy(l => l.Rank ?? int.MaxValue)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                })
                .ToList();
            
            _logger.LogInformation("Successfully populated {LocationCount} locations in model", model.LocationOptions?.Count ?? 0);
        }
    }

    private void SaveSearchPreferencesToCookie(SearchViewModel model)
    {
        var options = new CookieOptions
        {
            Expires = DateTime.Now.AddDays(30),
            IsEssential = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        };

        var preferences = System.Text.Json.JsonSerializer.Serialize(model);
        Response.Cookies.Append(SearchPreferencesCookieKey, preferences, options);
    }

    private SearchViewModel GetSearchPreferencesFromCookie()
    {
        try
        {
            var preferencesJson = Request.Cookies[SearchPreferencesCookieKey];
            if (!string.IsNullOrEmpty(preferencesJson))
            {
                return System.Text.Json.JsonSerializer.Deserialize<SearchViewModel>(preferencesJson);
            }
        }
        catch
        {
            Response.Cookies.Delete(SearchPreferencesCookieKey);
        }

        return null;
    }
}
