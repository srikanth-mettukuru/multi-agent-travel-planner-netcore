namespace MultiAgentTravelPlanner.Web.Models;

public class AttractionResult
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class AttractionResults
{
    public List<AttractionResult> Attractions { get; set; } = new();
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}