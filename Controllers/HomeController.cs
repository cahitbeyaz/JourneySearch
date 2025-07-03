using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Models;
using ObiletJourneySearch.Models.DTOs;
using ObiletJourneySearch.Models.ViewModels;
using ObiletJourneySearch.Services;

namespace ObiletJourneySearch.Controllers;

/// <summary>
/// Handles the home page and search form operations including bus locations and session management.
/// </summary>

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IObiletApiClient _apiClient;
    private readonly ISessionService _sessionService;
    private const string SearchPreferencesCookieKey = "SearchPreferences";

    public HomeController(
        ILogger<HomeController> logger,
        IObiletApiClient apiClient,
        ISessionService sessionService)
    {
        _logger = logger;
        _apiClient = apiClient;
        _sessionService = sessionService;
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
            // Get the current session or create a new one
            var session = await _sessionService.GetOrCreateSessionAsync();
            if (session == null)
            {
                _logger.LogError("Failed to create or retrieve session");
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
                model.DepartureDate = savedPreferences.DepartureDate;
            }
            else
            {
                // Default to tomorrow's date if no saved preferences
                model.DepartureDate = DateTime.Now.AddDays(1).Date;
            }

            try
            {
                // Get bus locations from API
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
                    _logger.LogError("Failed to get bus locations. Status: {Status}, Message: {Message}", 
                        locationsResponse.Status, locationsResponse.Message);
                    ViewBag.ErrorMessage = $"Failed to retrieve bus locations: {locationsResponse.Message ?? "Unknown error"}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bus locations");
                ViewBag.ErrorMessage = "An error occurred while retrieving bus locations. Please try again later.";
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Index action");
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
            // Remove ModelState errors for auxiliary fields that we don't care about
            RemoveAuxiliaryFieldErrors();
            
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

            if (model.DepartureDate < DateTime.Now.Date)
            {
                ViewBag.ErrorMessage = "Departure date cannot be in the past.";
                await PopulateLocationsInModel(model);
                return View("Index", model);
            }

            // Save search preferences to a cookie for future use
            SaveSearchPreferencesToCookie(model);

            // Redirect to the Journey controller's Index action with the search parameters
            return RedirectToAction("Index", "Journey", new
            {
                originId = model.OriginId,
                destinationId = model.DestinationId,
                departureDate = model.DepartureDate.ToString("yyyy-MM-dd")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search request");
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error swapping locations");
            TempData["ErrorMessage"] = "An error occurred while swapping locations.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Sets the departure date to either today or tomorrow based on user selection.
    /// </summary>
    /// <param name="dateType">Date selection type ("Today" or "Tomorrow")</param>
    /// <returns>Redirect to Index view with updated date</returns>
    [HttpPost]
    public IActionResult SetDate(string dateType)
    {
        try
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
                    model.DepartureDate = DateTime.Now.Date;
                    TempData["SuccessMessage"] = "Departure date set to today.";
                    break;
                case "tomorrow":
                    model.DepartureDate = DateTime.Now.Date.AddDays(1);
                    TempData["SuccessMessage"] = "Departure date set to tomorrow.";
                    break;
                default:
                    _logger.LogWarning("Invalid date type provided: {DateType}", dateType);
                    TempData["ErrorMessage"] = "Invalid date selection.";
                    break;
            }
            
            SaveSearchPreferencesToCookie(model);
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting date");
            TempData["ErrorMessage"] = "An error occurred while updating the departure date.";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = "An error occurred while processing your request. Please try again later."
        });
    }
    
    /// <summary>
    /// Helper method to populate location options in the search view model
    /// </summary>
    /// <param name="model">The model to populate with locations</param>
    private async Task PopulateLocationsInModel(SearchViewModel model)
    {
        try
        {
            var session = await _sessionService.GetOrCreateSessionAsync();
            if (session == null)
            {
                _logger.LogError("Failed to create or retrieve session during model population");
                return;
            }
            
            var locationsRequest = new BusLocationRequest
            {
                DeviceSession = session,
                Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Language = "en-EN"
            };
            
            var locationsResponse = await _apiClient.GetBusLocationsAsync(locationsRequest);
            
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
                _logger.LogError("Failed to get bus locations during model population");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating locations in model");
        }
    }

    /// <summary>
    /// Helper method to remove ModelState errors for auxiliary fields that aren't part of form validation
    /// </summary>
    private void RemoveAuxiliaryFieldErrors()
    {
        // Remove validation errors for all auxiliary fields
        ModelState.Remove("OriginName");
        ModelState.Remove("DestinationName");
        ModelState.Remove("SearchOrigin");
        ModelState.Remove("SearchDestination");
        ModelState.Remove("LocationOptions");
    }

    private void SaveSearchPreferencesToCookie(SearchViewModel model)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving search preferences to cookie");
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search preferences");
            Response.Cookies.Delete(SearchPreferencesCookieKey);
        }

        return null;
    }
}
