namespace MultiAgentTravelPlanner.Web.Models;

public class TravelItinerary
{
    public TravelRequest Request { get; set; } = new();
    public string ItineraryContent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}