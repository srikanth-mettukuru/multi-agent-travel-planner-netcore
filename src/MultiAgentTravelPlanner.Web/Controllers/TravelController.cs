using Microsoft.AspNetCore.Mvc;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Services;
using System.Diagnostics;

namespace MultiAgentTravelPlanner.Web.Controllers;

public class TravelController : Controller
{
    private readonly ITravelAgent _travelAgent;
    private readonly ILogger<TravelController> _logger;

    public TravelController(ITravelAgent travelAgent, ILogger<TravelController> logger)
    {
        _travelAgent = travelAgent;
        _logger = logger;
    }

    /// <summary>
    /// Shows the main travel planning form
    /// </summary>
    public IActionResult Index()
    {
        // Initialize with default values
        var model = new TravelRequest
        {
            Origin = "Seattle",
            Destination = "Tokyo",
            StartDate = DateTime.Today.AddDays(30),
            EndDate = DateTime.Today.AddDays(37)
        };

        return View(model);
    }

    /// <summary>
    /// Handles travel itinerary generation using the TravelAgent supervisor
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GenerateItinerary(TravelRequest request)
    {
        // Validate model
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for travel request");
            return View("Index", request);
        }

        // Additional validation
        if (string.IsNullOrWhiteSpace(request.Origin) || 
            string.IsNullOrWhiteSpace(request.Destination) ||
            request.StartDate == default ||
            request.EndDate == default)
        {
            ModelState.AddModelError("", "Please fill in all fields.");
            return View("Index", request);
        }

        if (request.EndDate <= request.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be after start date.");
            return View("Index", request);
        }

        if (request.StartDate < DateTime.Today)
        {
            ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            return View("Index", request);
        }

        _logger.LogInformation("Processing travel request from {Origin} to {Destination} " +
                             "between {StartDate} and {EndDate}",
            request.Origin, request.Destination, request.StartDate, request.EndDate);

        try
        {
            // Call the TravelAgent supervisor to orchestrate all agents
            var itinerary = await _travelAgent.PlanTripAsync(request);

            if (itinerary.IsSuccessful)
            {
                _logger.LogInformation("Successfully generated itinerary for {Origin} to {Destination}. " +
                    "Flights: {FlightCount}, Hotels: {HotelCount}, Attractions: {AttractionCount}, Restaurants: {RestaurantCount}",
                    request.Origin, request.Destination,
                    itinerary.Flights?.OutboundFlights.Count ?? 0,
                    itinerary.Hotels?.Hotels.Count ?? 0,
                    itinerary.Attractions?.Attractions.Count ?? 0,
                    itinerary.Restaurants?.Restaurants.Count ?? 0);
                
                // Show results page with complete itinerary
                return View("Result", itinerary);
            }
            else
            {
                _logger.LogWarning("Failed to generate itinerary: {ErrorMessage}", itinerary.ErrorMessage);
                
                // Add error to model and return to form
                ModelState.AddModelError("", itinerary.ErrorMessage ?? "Unable to generate itinerary. Please try again.");
                return View("Index", request);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating itinerary for {Origin} to {Destination}",
                request.Origin, request.Destination);
            
            // Handle errors gracefully
            ModelState.AddModelError("", "An unexpected error occurred while planning your trip. Please try again.");
            return View("Index", request);
        }
    }

    /// <summary>
    /// Shows the error page (standard ASP.NET Core error handling)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}