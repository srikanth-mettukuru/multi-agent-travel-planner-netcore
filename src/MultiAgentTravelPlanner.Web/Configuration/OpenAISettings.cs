namespace MultiAgentTravelPlanner.Web.Configuration;

public class OpenAISettings
{
    public const string SectionName = "OpenAI";    
    
    /// Maps to OPENAI_API_KEY environment variable
    public string ApiKey { get; set; } = string.Empty;
    
    /// Maps to OPENAI_MODEL environment variable
    public string Model { get; set; } = string.Empty;    

    /// Validates if all required settings are configured
    public bool IsValid => 
        !string.IsNullOrEmpty(ApiKey) && 
        !string.IsNullOrEmpty(Model);  
}