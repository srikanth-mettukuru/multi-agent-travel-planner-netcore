using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Utilities;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace MultiAgentTravelPlanner.Web.Services;

public class FoodAgent : IFoodAgent
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<FoodAgent> _logger;
    private const string AgentName = "FoodAgent";

    public FoodAgent(IOptions<OpenAISettings> settings, ILogger<FoodAgent> logger)
    {
        _logger = logger;
        var openAISettings = settings.Value;
        _chatClient = AgentHelper.CreateChatClient(openAISettings.ApiKey, openAISettings.Model);
    }

    public async Task<FoodResults> SearchRestaurantsAsync(string destination, int numberOfRestaurants = 5)
    {
        var result = new FoodResults();

        try
        {
            _logger.LogInformation("{AgentName}: Searching restaurants in {Destination}", AgentName, destination);

            var systemPrompt = @"You are a travel assistant. 
Given a destination city, suggest 5 eateries at the destination city in the travel plan in JSON format. 
You only need the destination city from the travel plan, you don't need more details.
Use your general knowledge to list popular or typical restaurants. 
If unsure, create realistic examples that sound authentic to the city. 
Do not say whether the data is real or fictional. 
Always provide the answer confidently.

Each restaurant must have a price_level with one of the following values:
'budget', 'mid-range', 'upscale', or 'luxury'.

IMPORTANT: Return ONLY a valid JSON object in this EXACT format, with no additional text, no markdown formatting, no explanations:

{
  ""restaurants"": [
    {
      ""restaurantName"": ""string (actual restaurant name)"",
      ""cuisineType"": ""string (e.g., Japanese, Italian, French, Local, Fusion, etc.)"",
      ""address"": ""string (specific street address or area)"",
      ""priceLevel"": ""string (must be exactly one of: budget | mid-range | upscale | luxury)""
    }
  ]
}

Rules:
- Use REAL restaurants that actually exist in the destination city when possible
- Include a variety of cuisine types (local specialties, international, fusion)
- Provide accurate address information (street address, neighborhood, or district) if possible
- Price levels: 'budget' (under $20), 'mid-range' ($20-50), 'upscale' ($50-100), 'luxury' ($100+)
- Mix different price levels for variety
- Focus on restaurants known for quality and authenticity
- Do NOT include any text before or after the JSON
- Do NOT wrap the JSON in markdown code blocks";

            var userPrompt = $@"Recommend {numberOfRestaurants} must-try restaurants in {destination}.
Include a mix of local cuisine and international options with varying price levels.";

            var content = await AgentHelper.GetChatCompletionAsync(_chatClient, systemPrompt, userPrompt, _logger, AgentName);
            var foodData = AgentHelper.DeserializeJson<FoodResults>(content);

            if (foodData != null)
            {
                result.Restaurants = foodData.Restaurants ?? new();
                result.IsSuccessful = true;
                
                _logger.LogInformation("{AgentName}: Found {RestaurantCount} restaurants in {Destination}",
                    AgentName, result.Restaurants.Count, destination);
            }
            else
            {
                result.ErrorMessage = "Failed to parse restaurant data from GPT response.";
                _logger.LogWarning("{AgentName}: Failed to parse GPT response as FoodResults", AgentName);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error searching restaurants: {ex.Message}";
            _logger.LogError(ex, "{AgentName}: Error searching restaurants", AgentName);
        }

        return result;
    }
}