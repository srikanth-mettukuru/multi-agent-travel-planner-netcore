using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface IFlightsAgent
{
    Task<FlightResults> SearchFlightsAsync(string origin, string destination, DateTime departureDate, DateTime? returnDate = null);
}