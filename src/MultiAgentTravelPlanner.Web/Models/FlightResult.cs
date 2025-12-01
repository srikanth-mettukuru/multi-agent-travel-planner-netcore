namespace MultiAgentTravelPlanner.Web.Models;

public class FlightResult
{
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public string Class { get; set; } = "Economy";
    public int Stops { get; set; }
}

public class FlightResults
{
    public List<FlightResult> OutboundFlights { get; set; } = new();
    public List<FlightResult> ReturnFlights { get; set; } = new();
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}