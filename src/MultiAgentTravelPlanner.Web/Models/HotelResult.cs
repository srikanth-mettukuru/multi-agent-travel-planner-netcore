namespace MultiAgentTravelPlanner.Web.Models;

public class HotelResult
{
    public string HotelName { get; set; } = string.Empty;    
    public string Address { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }    
}

public class HotelResults
{
    public List<HotelResult> Hotels { get; set; } = new();
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}