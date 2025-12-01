namespace MultiAgentTravelPlanner.Web.Models;

public class TravelItinerary
{
    public TravelRequest Request { get; set; } = new();
    public FlightResults? Flights { get; set; }
    public HotelResults? Hotels { get; set; }
    public AttractionResults? Attractions { get; set; }
    public FoodResults? Restaurants { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}