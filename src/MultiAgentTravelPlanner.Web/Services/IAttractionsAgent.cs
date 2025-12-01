using MultiAgentTravelPlanner.Web.Models;

namespace MultiAgentTravelPlanner.Web.Services;

public interface IAttractionsAgent
{
    Task<AttractionResults> SearchAttractionsAsync(string destination, int numberOfAttractions = 5);
}
