using Microsoft.AspNetCore.Mvc;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Services;
using System.Diagnostics;

namespace MultiAgentTravelPlanner.Web.Controllers;

public class TravelController : Controller
{
    private readonly IAzureAITravelService _azureAITravelService;
    private readonly ILogger<TravelController> _logger;

    public TravelController(IAzureAITravelService azureAITravelService, ILogger<TravelController> logger)
    {
        _azureAITravelService = azureAITravelService;
        _logger = logger;
    }

    /// <summary>
    /// Shows the main travel planning form
    /// </summary>
    public IActionResult Index()
    {
        // Initialize with default values (like your Python placeholders)
        var model = new TravelRequest
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        return View(model);
    }

    /// <summary>
    /// Handles travel itinerary generation   
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

        _logger.LogInformation("Processing travel request from {Origin} to {Destination} " +
                             "between {StartDate} and {EndDate}",
            request.Origin, request.Destination, request.StartDate, request.EndDate);

        try
        {
            // Call the Azure AI service (equivalent to your Python: with st.spinner(...))
            var itinerary = await _azureAITravelService.GenerateItineraryAsync(request);

            if (itinerary.IsSuccessful)
            {
                _logger.LogInformation("Successfully generated itinerary for {Origin} to {Destination}",
                    request.Origin, request.Destination);
                
                // Show results page (equivalent to your Python: st.success and st.markdown)
                return View("Result", itinerary);
            }
            else
            {
                _logger.LogWarning("Failed to generate itinerary: {ErrorMessage}", itinerary.ErrorMessage);
                
                // Add error to model and return to form (equivalent to your Python: st.error)
                ModelState.AddModelError("", itinerary.ErrorMessage ?? "Unable to generate itinerary. Please try again.");
                return View("Index", request);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating itinerary for {Origin} to {Destination}",
                request.Origin, request.Destination);
            
            // Handle errors (equivalent to your Python: except Exception as e: st.error)
            ModelState.AddModelError("", "An unexpected error occurred. Please try again or check your connection.");
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