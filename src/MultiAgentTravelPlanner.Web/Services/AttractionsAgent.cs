using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Utilities;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace MultiAgentTravelPlanner.Web.Services;

public class AttractionsAgent : IAttractionsAgent
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AttractionsAgent> _logger;
    private const string AgentName = "AttractionsAgent";

    public AttractionsAgent(IOptions<OpenAISettings> settings, ILogger<AttractionsAgent> logger)
    {
        _logger = logger;
        var openAISettings = settings.Value;
        _chatClient = new ChatClient(openAISettings.Model, new ApiKeyCredential(openAISettings.ApiKey));
    }

    public async Task<AttractionResults> SearchAttractionsAsync(string destination, int numberOfAttractions = 3)
    {
        var result = new AttractionResults();

        try
        {
            _logger.LogInformation("{AgentName}: Searching attractions in {Destination}", AgentName, destination);

            var systemPrompt = @"You are a travel assistant. 
Given a destination city, return 3 visitor attractions at the destination city in JSON format that includes 'attraction name' and 'location'.
Do not ask for preferences or other questions.
If you have reliable knowledge about the city, use real attraction names and locations.
If real data about the city is known, use it; otherwise create plausible, realistic attractions without mentioning that they are mock or fictional.

IMPORTANT: Return ONLY a valid JSON object in this EXACT format, with no additional text, no markdown formatting, no explanations:

{
  ""attractions"": [
    {
      ""name"": ""string (actual attraction name)"",
      ""location"": ""string (specific address or area within the city)""
    }
  ]
}

Rules:
- Use REAL attractions that actually exist in the destination city when possible
- Include famous landmarks, museums, parks, cultural sites, shopping areas, etc.
- Provide accurate location information (neighborhood, district, street or full address if known)
- Do NOT include any text before or after the JSON
- Do NOT wrap the JSON in markdown code blocks";

            var userPrompt = $@"Recommend {numberOfAttractions} must-visit tourist attractions in {destination}.
Include a mix of cultural, historical, and entertainment options.";

            var content = await AgentHelper.GetChatCompletionAsync(_chatClient, systemPrompt, userPrompt, _logger, AgentName);
            var attractionData = AgentHelper.DeserializeJson<AttractionResults>(content);

            if (attractionData != null)
            {
                result.Attractions = attractionData.Attractions ?? new();
                result.IsSuccessful = true;
                
                _logger.LogInformation("{AgentName}: Found {AttractionCount} attractions in {Destination}",
                    AgentName, result.Attractions.Count, destination);
            }
            else
            {
                result.ErrorMessage = "Failed to parse attraction data from GPT response.";
                _logger.LogWarning("{AgentName}: Failed to parse GPT response as AttractionResults", AgentName);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error searching attractions: {ex.Message}";
            _logger.LogError(ex, "{AgentName}: Error searching attractions", AgentName);
        }

        return result;
    }
}