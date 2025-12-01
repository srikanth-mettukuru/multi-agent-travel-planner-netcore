using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using MultiAgentTravelPlanner.Web.Utilities;
using OpenAI.Chat;
using System.Text;

namespace MultiAgentTravelPlanner.Web.Services;

public class TravelAgent : ITravelAgent
{
    private readonly IFlightsAgent _flightsAgent;
    private readonly IHotelsAgent _hotelsAgent;
    private readonly IAttractionsAgent _attractionsAgent;
    private readonly IFoodAgent _foodAgent;
    private readonly ChatClient _chatClient;
    private readonly ILogger<TravelAgent> _logger;
    private const string AgentName = "TravelAgent";

    public TravelAgent(
        IFlightsAgent flightsAgent,
        IHotelsAgent hotelsAgent,
        IAttractionsAgent attractionsAgent,
        IFoodAgent foodAgent,
        IOptions<OpenAISettings> settings,
        ILogger<TravelAgent> logger)
    {
        _flightsAgent = flightsAgent;
        _hotelsAgent = hotelsAgent;
        _attractionsAgent = attractionsAgent;
        _foodAgent = foodAgent;
        _logger = logger;

        var openAISettings = settings.Value;
        _chatClient = AgentHelper.CreateChatClient(openAISettings.ApiKey, openAISettings.Model);
    }

    public async Task<TravelItinerary> PlanTripAsync(TravelRequest request)
    {
        var itinerary = new TravelItinerary
        {
            Request = request,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("{AgentName}: Planning trip from {Origin} to {Destination} ({StartDate} to {EndDate})",
                AgentName, request.Origin, request.Destination, request.StartDate, request.EndDate);

            // Step 1: Invoke all agents in parallel for efficiency
            _logger.LogInformation("{AgentName}: Invoking all specialized agents...", AgentName);

            var flightsTask = _flightsAgent.SearchFlightsAsync(
                request.Origin,
                request.Destination,
                request.StartDate,
                request.EndDate);

            var hotelsTask = _hotelsAgent.SearchHotelsAsync(
                request.Destination,
                request.StartDate,
                request.EndDate);

            var attractionsTask = _attractionsAgent.SearchAttractionsAsync(
                request.Destination,
                numberOfAttractions: 3);

            var restaurantsTask = _foodAgent.SearchRestaurantsAsync(
                request.Destination,
                numberOfRestaurants: 5);

            // Wait for all agents to complete
            await Task.WhenAll(flightsTask, hotelsTask, attractionsTask, restaurantsTask);

            // Collect results
            itinerary.Flights = await flightsTask;
            itinerary.Hotels = await hotelsTask;
            itinerary.Attractions = await attractionsTask;
            itinerary.Restaurants = await restaurantsTask;

            _logger.LogInformation("{AgentName}: All agents completed. Flights: {FlightSuccess}, Hotels: {HotelSuccess}, Attractions: {AttractionSuccess}, Restaurants: {RestaurantSuccess}",
                AgentName,
                itinerary.Flights.IsSuccessful,
                itinerary.Hotels.IsSuccessful,
                itinerary.Attractions.IsSuccessful,
                itinerary.Restaurants.IsSuccessful);

            // Step 2: Check if all agents succeeded
            if (!itinerary.Flights.IsSuccessful || !itinerary.Hotels.IsSuccessful ||
                !itinerary.Attractions.IsSuccessful || !itinerary.Restaurants.IsSuccessful)
            {
                var errors = new List<string>();
                if (!itinerary.Flights.IsSuccessful) errors.Add($"Flights: {itinerary.Flights.ErrorMessage}");
                if (!itinerary.Hotels.IsSuccessful) errors.Add($"Hotels: {itinerary.Hotels.ErrorMessage}");
                if (!itinerary.Attractions.IsSuccessful) errors.Add($"Attractions: {itinerary.Attractions.ErrorMessage}");
                if (!itinerary.Restaurants.IsSuccessful) errors.Add($"Restaurants: {itinerary.Restaurants.ErrorMessage}");

                itinerary.ErrorMessage = $"Some agents failed: {string.Join("; ", errors)}";
                itinerary.IsSuccessful = false;
                _logger.LogWarning("{AgentName}: Trip planning partially failed: {Errors}", AgentName, itinerary.ErrorMessage);
                return itinerary;
            }

            // Step 3: Generate a comprehensive travel summary using GPT
            _logger.LogInformation("{AgentName}: Generating travel summary...", AgentName);
            itinerary.Summary = await GenerateTravelSummaryAsync(itinerary);

            itinerary.IsSuccessful = true;
            _logger.LogInformation("{AgentName}: Trip planning completed successfully", AgentName);
        }
        catch (Exception ex)
        {
            itinerary.ErrorMessage = $"Error planning trip: {ex.Message}";
            itinerary.IsSuccessful = false;
            _logger.LogError(ex, "{AgentName}: Error planning trip", AgentName);
        }

        return itinerary;
    }

    private async Task<string> GenerateTravelSummaryAsync(TravelItinerary itinerary)
    {
        try
        {
            var request = itinerary.Request;
            var nights = (request.EndDate - request.StartDate).Days;

            // Build context from all agent results
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine($"Trip Details:");
            contextBuilder.AppendLine($"- Origin: {request.Origin}");
            contextBuilder.AppendLine($"- Destination: {request.Destination}");
            contextBuilder.AppendLine($"- Dates: {request.StartDate:MMM dd} - {request.EndDate:MMM dd, yyyy} ({nights} nights)");
            contextBuilder.AppendLine();

            // Flights
            contextBuilder.AppendLine("Available Flights:");
            if (itinerary.Flights?.OutboundFlights.Any() == true)
            {
                foreach (var flight in itinerary.Flights.OutboundFlights.Take(3))
                {
                    contextBuilder.AppendLine($"- {flight.Airline} {flight.FlightNumber}: {flight.DepartureAirport} → {flight.ArrivalAirport}, ${flight.Price}, {flight.Class}");
                }
            }
            contextBuilder.AppendLine();

            // Hotels
            contextBuilder.AppendLine("Hotels:");
            if (itinerary.Hotels?.Hotels.Any() == true)
            {
                foreach (var hotel in itinerary.Hotels.Hotels)
                {
                    contextBuilder.AppendLine($"- {hotel.HotelName} ({hotel.StarRating}★): ${hotel.PricePerNight}/night");
                }
            }
            contextBuilder.AppendLine();

            // Attractions
            contextBuilder.AppendLine("Top Attractions:");
            if (itinerary.Attractions?.Attractions.Any() == true)
            {
                foreach (var attraction in itinerary.Attractions.Attractions)
                {
                    contextBuilder.AppendLine($"- {attraction.Name} : {attraction.Location}");
                }
            }
            contextBuilder.AppendLine();

            // Restaurants
            contextBuilder.AppendLine("Recommended Restaurants:");
            if (itinerary.Restaurants?.Restaurants.Any() == true)
            {
                foreach (var restaurant in itinerary.Restaurants.Restaurants)
                {
                    contextBuilder.AppendLine($"- {restaurant.RestaurantName} ({restaurant.CuisineType}): {restaurant.PriceLevel}");
                }
            }

            var systemPrompt = @"You are a professional travel planner. Based on the provided trip details and options, create a comprehensive, well-structured travel itinerary summary.

Your summary should:
- Start with a warm, engaging introduction
- Provide a day-by-day itinerary suggestion (distribute attractions across days)
- Recommend which flight option to take and why
- Suggest which hotel suits best based on budget/luxury preference
- Integrate restaurant recommendations into the daily schedule
- Include practical travel tips
- End with an encouraging closing statement

Format the response in clear sections with headers. Make it informative, engaging, and easy to read.
Do NOT use markdown formatting. Use plain text with clear section breaks and bullet points using hyphens.";

            var userPrompt = $@"Create a detailed travel itinerary summary for this trip:

{contextBuilder}

Generate a comprehensive, day-by-day itinerary that includes flight recommendations, hotel selection advice, and a schedule incorporating the attractions and restaurants.";

            var summary = await AgentHelper.GetChatCompletionAsync(_chatClient, systemPrompt, userPrompt, _logger, AgentName);

            _logger.LogInformation("{AgentName}: Generated travel summary ({Length} characters)", AgentName, summary.Length);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{AgentName}: Error generating travel summary", AgentName);
            return "Unable to generate travel summary. Please review the individual components above.";
        }
    }
}