namespace MultiAgentTravelPlanner.Web.Models;

public class FoodResult
{
    public string RestaurantName { get; set; } = string.Empty;
    public string CuisineType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PriceLevel { get; set; } = string.Empty; // budget | mid-range | upscale | luxury
}

public class FoodResults
{
    public List<FoodResult> Restaurants { get; set; } = new();
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}