using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface IHotelsAgent
{
    Task<HotelResults> SearchHotelsAsync(string destination, DateTime checkInDate, DateTime checkOutDate);
}