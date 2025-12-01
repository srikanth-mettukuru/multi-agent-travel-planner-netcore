using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface ITravelAgent
{
    Task<TravelItinerary> PlanTripAsync(TravelRequest request);
}