using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface IFoodAgent
{
    Task<FoodResults> SearchRestaurantsAsync(string destination, int numberOfRestaurants = 5);
}