namespace MultiAgentTravelPlanner.Web.Configuration;

public class AzureAISettings
{
    public const string SectionName = "AzureAI";    
    
    /// Maps to AZURE_AI_PROJECT_ENDPOINT environment variable  
    public string ProjectEndpoint { get; set; } = string.Empty;
    
    
    /// Maps to AZURE_AI_PROJECT_NAME environment variable    
    public string ProjectName { get; set; } = string.Empty;
    
    
    /// Maps to AZURE_AI_AGENT_ID environment variable  
    public string AgentId { get; set; } = string.Empty;
    
    
    /// Maps to AZURE_TENANT_ID environment variable (optional)   
    public string? TenantId { get; set; }
    
   
    /// Maps to AZURE_CLIENT_ID environment variable (optional)  
    public string? ClientId { get; set; }
    
 
    /// Maps to AZURE_CLIENT_SECRET environment variable (optional)
    public string? ClientSecret { get; set; }
    

    /// Validates if all required settings are configured
    public bool IsValid => 
        !string.IsNullOrEmpty(ProjectEndpoint) && 
        !string.IsNullOrEmpty(ProjectName) && 
        !string.IsNullOrEmpty(AgentId);  


    /// Checks if service principal credentials are provided
    public bool HasServicePrincipalCredentials => 
        !string.IsNullOrEmpty(TenantId) && 
        !string.IsNullOrEmpty(ClientId) && 
        !string.IsNullOrEmpty(ClientSecret);
    
}