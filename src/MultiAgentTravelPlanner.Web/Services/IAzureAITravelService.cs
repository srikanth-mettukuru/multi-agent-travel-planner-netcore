using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface IAzureAITravelService
{
    /// <summary>
    /// Generates a travel itinerary using the multi-agent system
    /// </summary>
    /// <param name="request">Travel request containing origin, destination, and dates</param>
    /// <returns>Complete travel itinerary with flights, hotels, attractions, and restaurants</returns>
    Task<TravelItinerary> GenerateItineraryAsync(TravelRequest request);
}