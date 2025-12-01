using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Utilities;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace MultiAgentTravelPlanner.Web.Services;

public class FlightsAgent : IFlightsAgent
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<FlightsAgent> _logger;
    private const string AgentName = "FlightsAgent";

    public FlightsAgent(IOptions<OpenAISettings> settings, ILogger<FlightsAgent> logger)
    {
        _logger = logger;
        var openAISettings = settings.Value;
        _chatClient = AgentHelper.CreateChatClient(openAISettings.ApiKey, openAISettings.Model);
    }

    public async Task<FlightResults> SearchFlightsAsync(string origin, string destination, DateTime departureDate, DateTime? returnDate = null)
    {
        var result = new FlightResults();

        try
        {
            _logger.LogInformation("{AgentName}: Searching flights from {Origin} to {Destination} on {DepartureDate}", 
                AgentName, origin, destination, departureDate);

            var systemPrompt = @"You are a travel assistant.  
Given home city, destination, and travel dates, return **mock** flight options in JSON format.  
Use realistic-sounding but fictional airline and airport names for the mock flight options.  
Do not mention that it is mocked flight data, just return the data confidently
Include both outbound and return-trip options (3 each).

IMPORTANT: Return ONLY a valid JSON object in this EXACT format, with no additional text, no markdown formatting, no explanations:

{
  ""outboundFlights"": [
    {
      ""airline"": ""string"",
      ""flightNumber"": ""string"",
      ""departureAirport"": ""string (3-letter code)"",
      ""arrivalAirport"": ""string (3-letter code)"",
      ""departureTime"": ""ISO 8601 datetime string"",
      ""arrivalTime"": ""ISO 8601 datetime string"",
      ""price"": number,
      ""class"": ""string (Economy/Business/First)"",
      ""stops"": number
    }
  ],
  ""returnFlights"": [
    {
      ""airline"": ""string"",
      ""flightNumber"": ""string"",
      ""departureAirport"": ""string (3-letter code)"",
      ""arrivalAirport"": ""string (3-letter code)"",
      ""departureTime"": ""ISO 8601 datetime string"",
      ""arrivalTime"": ""ISO 8601 datetime string"",
      ""price"": number,
      ""class"": ""string (Economy/Business/First)"",
      ""stops"": number
    }
  ]
}

Rules:
- Generate 3 realistic options for each direction
- Use realistic airline codes (AA, UA, DL, BA, LH, etc.) and airport codes (JFK, LAX, LHR, CDG, etc.)
- Prices should be realistic USD amounts (no currency symbols, just numbers)
- departureTime and arrivalTime must be valid ISO 8601 format (e.g., ""2025-12-20T10:30:00"")
- For one-way trips, returnFlights should be an empty array
- Do NOT include any text before or after the JSON
- Do NOT wrap the JSON in markdown code blocks";

            var userPrompt = $@"Find flights from {origin} to {destination}.
Departure date: {departureDate:yyyy-MM-dd}
{(returnDate.HasValue ? $"Return date: {returnDate.Value:yyyy-MM-dd}" : "One-way trip")}";

            var content = await AgentHelper.GetChatCompletionAsync(_chatClient, systemPrompt, userPrompt, _logger, AgentName);
            var flightData = AgentHelper.DeserializeJson<FlightResults>(content);            

            if (flightData != null)
            {
                result.OutboundFlights = flightData.OutboundFlights ?? new();
                result.ReturnFlights = flightData.ReturnFlights ?? new();
                result.IsSuccessful = true;
                
                _logger.LogInformation("{AgentName}: Found {OutboundCount} outbound and {ReturnCount} return flights",
                                        AgentName, result.OutboundFlights.Count, result.ReturnFlights.Count);
            }
            else
            {
                result.ErrorMessage = "Failed to parse flight data from GPT response.";
                _logger.LogWarning("{AgentName}: Failed to parse GPT response as FlightResults", AgentName);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error searching flights: {ex.Message}";
            _logger.LogError(ex, "{AgentName}: Error searching flights", AgentName);
        }

        return result;
    }
}