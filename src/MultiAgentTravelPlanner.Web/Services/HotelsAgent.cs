using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Utilities;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace MultiAgentTravelPlanner.Web.Services;

public class HotelsAgent : IHotelsAgent
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<HotelsAgent> _logger;
    private const string AgentName = "HotelsAgent";

    public HotelsAgent(IOptions<OpenAISettings> settings, ILogger<HotelsAgent> logger)
    {
        _logger = logger;
        var openAISettings = settings.Value;
        _chatClient = AgentHelper.CreateChatClient(openAISettings.ApiKey, openAISettings.Model);
    }

    public async Task<HotelResults> SearchHotelsAsync(string destination, DateTime checkInDate, DateTime checkOutDate)
    {
        var result = new HotelResults();

        try
        {
            _logger.LogInformation("{AgentName}: Searching hotels in {Destination}", 
                AgentName, destination);

            var systemPrompt = @"You are a travel assistant.
Given a destination city and travel dates, generate 3 mock hotel options in JSON format.
The hotels should have realistic-sounding but fictional names.
It is acceptable to invent plausible data; do not worry about factual accuracy.
Do not mention that it is mock data and just return the data confidently.
Do not ask questions or add any explanations.

IMPORTANT: Return ONLY a valid JSON object in this EXACT format, with no additional text, no markdown formatting, no explanations:

{
  ""hotels"": [
    {
      ""hotelName"": ""string"",
      ""address"": ""string"",
      ""starRating"": number (1-5),
      ""pricePerNight"": number
    }
  ]
}

Rules:
- Generate exactly 3 hotel options with varying price points (budget, mid-range, luxury)
- Star ratings should be between 1-5
- Price per night should be realistic for the destination (in USD, no currency symbols)
- Addresses should include street, city, and be appropriate for the destination
- Do NOT include any text before or after the JSON
- Do NOT wrap the JSON in markdown code blocks";

            var userPrompt = $@"Find hotels in {destination}.
Check-in date: {checkInDate:yyyy-MM-dd}
Check-out date: {checkOutDate:yyyy-MM-dd}";

            var content = await AgentHelper.GetChatCompletionAsync(_chatClient, systemPrompt, userPrompt, _logger, AgentName);
            var hotelData = AgentHelper.DeserializeJson<HotelResults>(content);

            if (hotelData != null)
            {
                result.Hotels = hotelData.Hotels ?? new();
                result.IsSuccessful = true;
                
                _logger.LogInformation("{AgentName}: Found {HotelCount} hotels in {Destination}",
                    AgentName, result.Hotels.Count, destination);
            }
            else
            {
                result.ErrorMessage = "Failed to parse hotel data from GPT response.";
                _logger.LogWarning("{AgentName}: Failed to parse GPT response as HotelResults", AgentName);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error searching hotels: {ex.Message}";
            _logger.LogError(ex, "{AgentName}: Error searching hotels", AgentName);
        }

        return result;
    }
}