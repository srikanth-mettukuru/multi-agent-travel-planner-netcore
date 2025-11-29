using Azure.AI.Projects;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Assistants;

using Azure.Identity;
using Microsoft.Extensions.Options;
using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Models;
using OpenAI.Assistants;

namespace MultiAgentTravelPlanner.Web.Services;

public class AzureAITravelService : IAzureAITravelService
{
        private readonly AzureAISettings _settings;
        private readonly AIProjectClient _projectClient;
    #pragma warning disable OPENAI001
        private readonly AssistantClient _assistantClient;
    #pragma warning restore OPENAI001
    
    
        private readonly ILogger<AzureAITravelService> _logger;

    public AzureAITravelService(IOptions<AzureAISettings> settings, ILogger<AzureAITravelService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Validate configuration
        if (!_settings.IsValid)
        {
            throw new InvalidOperationException(
                "Azure AI configuration is incomplete. Please check your appsettings.json or environment variables.");
        }

        // Initialize Azure credential
        var credential = CreateAzureCredential();

        var azureOpenAIClient = new AzureOpenAIClient(new Uri(_settings.ProjectEndpoint), credential);

    #pragma warning disable OPENAI001
        _assistantClient = azureOpenAIClient.GetAssistantClient();
    #pragma warning restore OPENAI001
       
        _projectClient = new AIProjectClient(new Uri(_settings.ProjectEndpoint), credential);

        _logger.LogInformation("Azure AI Service initialized with endpoint: {Endpoint}", _settings.ProjectEndpoint);
    }

    public async Task<TravelItinerary> GenerateItineraryAsync(TravelRequest request)
    {
        var itinerary = new TravelItinerary { Request = request };

        try
        {
            // Create the user prompt
            var userPrompt = $"I want to travel from {request.Origin} to {request.Destination} " +
                           $"between {request.StartDate:yyyy-MM-dd} and {request.EndDate:yyyy-MM-dd}. " +
                           $"Generate a detailed travel itinerary with flights, hotels, attractions, and restaurants.";

            _logger.LogInformation("Generating itinerary for travel from {Origin} to {Destination} " +
                                 "between {StartDate} and {EndDate}",
                request.Origin, request.Destination, request.StartDate, request.EndDate);

            /*

            // Get the agents client to interact with the Azure AI agents
            _logger.LogInformation("Getting agents client...");
            var agentsClient = _projectClient.GetAgentsClient();

            // Create thread and run
            try
            {
                _logger.LogInformation("Creating thread...");
                var threadResponse = await agentsClient.CreateThreadAsync();
                _logger.LogInformation("Thread creation response received");
                var thread = threadResponse.Value;
                _logger.LogInformation("Thread created: {ThreadId}", thread.Id);

                // Add message to thread
                _logger.LogInformation("Adding message to thread...");
                await agentsClient.CreateMessageAsync(
                    thread.Id,
                    MessageRole.User,
                    userPrompt);

                // Create and start the run
                _logger.LogInformation("Creating run with thread: {ThreadId}, agent: {AgentId}", thread.Id, _settings.AgentId);
                var runResponse = await agentsClient.CreateRunAsync(
                    thread.Id,
                    _settings.AgentId);
                var run = runResponse.Value;

                _logger.LogInformation("Created run for {ThreadId} with run {RunId}", thread.Id, run.Id);

                */

            try
            {                
                var assistant = await _assistantClient.GetAssistantAsync(_settings.AgentId);
                var thread = await _assistantClient.CreateThreadAsync();
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                await _assistantClient.CreateMessageAsync(thread.Value.Id, MessageRole.User, new[]{MessageContent.FromText(userPrompt)});
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var run = (await _assistantClient.CreateRunAsync(thread.Value.Id, _settings.AgentId)).Value;

                // Wait for completion
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
                {
                    await Task.Delay(2000); // 2-second delay between status checks
                    var runStatusResponse = await _assistantClient.GetRunAsync(run.ThreadId, run.Id);
                    run = runStatusResponse.Value;
                    
                    _logger.LogDebug("Run status: {Status}", run.Status);
                }
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                _logger.LogInformation("Run completed with status: {Status}", run.Status);


                // Get the response messages
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                if (run.Status == RunStatus.Completed)
                {
                    // Get the response messages
                    var messages = _assistantClient.GetMessagesAsync(thread.Value.Id);

                    await foreach (var message in messages)
                    {
                        if (message.Role != MessageRole.Assistant)
                        {
                            continue;
                        }  

                        var text = string.Concat(
                                message.Content                               
                                .Select(p => p.Text)
                                .Where(segment => !string.IsNullOrWhiteSpace(segment)));                      

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            itinerary.ItineraryContent = text;
                            itinerary.IsSuccessful = true;
                            break;
                        }

                        if (itinerary.IsSuccessful)
                        {
                            break;
                        }
                    }

                    /* // Find assistant messages
                    var assistantMessages = messages
                        .Where(msg => msg.Role == MessageRole.Assistant)
                        .ToList();

                    if (assistantMessages.Any())
                    {
                        var assistantMessage = assistantMessages.First();
                        
                        if (assistantMessage.ContentItems.Any())
                        {
                            var messageContent = assistantMessage.ContentItems.First();

                            if (messageContent is MessageTextContent textContent)
                            {
                                itinerary.ItineraryContent = textContent.Text;
                                itinerary.IsSuccessful = true;
                                _logger.LogInformation("Successfully generated itinerary with {ContentLength} characters", 
                                    textContent.Text.Length);
                            }
                            else
                            {
                                itinerary.ErrorMessage = "Received non-text content from the AI agent.";
                                _logger.LogWarning("Assistant message content was not text");
                            }
                        }
                        else
                        {
                            itinerary.ErrorMessage = "Assistant message had no content.";
                            _logger.LogWarning("Assistant message found but content was empty");
                        }
                    }
                    else
                    {
                        itinerary.ErrorMessage = "No assistant response received from the AI agent.";
                        _logger.LogWarning("No assistant messages found in response");
                    } */
                }
                else
                {
                    itinerary.ErrorMessage = $"AI agent run failed with status: {run.Status}. " +
                    (!string.IsNullOrEmpty(run.LastError?.Message) ? $"Error: {run.LastError.Message}" : "");
                    _logger.LogWarning("Run completed with non-success status: {Status}", run.Status);
                }
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }
            catch (Azure.RequestFailedException azEx)
            {
                _logger.LogError(azEx, "Azure Request Failed - Status: {Status}, ErrorCode: {ErrorCode}, Message: {Message}", 
                    azEx.Status, azEx.ErrorCode, azEx.Message);
                itinerary.ErrorMessage = $"Azure API error: {azEx.Message}";
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP Request Failed - Message: {Message}, InnerException: {InnerException}", 
                    httpEx.Message, httpEx.InnerException?.Message);
                itinerary.ErrorMessage = $"Network error: {httpEx.Message}. Please check your connection and endpoint URL.";
            }            
        }
        catch (Exception ex)
        {
            itinerary.ErrorMessage = $"An error occurred: {ex.Message}";
            _logger.LogError(ex, "Error generating itinerary for {Origin} to {Destination}", 
                request.Origin, request.Destination);
        }

        return itinerary;
    }

    /// Checks for service principal credentials first, falls back to DefaultAzureCredential
    private Azure.Core.TokenCredential CreateAzureCredential()
    {
        // Equivalent to your Python: if os.getenv('AZURE_CLIENT_ID') and os.getenv('AZURE_CLIENT_SECRET')...
        if (_settings.HasServicePrincipalCredentials)
        {
            _logger.LogInformation("Using ClientSecretCredential with service principal");
            return new ClientSecretCredential(
                tenantId: _settings.TenantId!,
                clientId: _settings.ClientId!,
                clientSecret: _settings.ClientSecret!
            );
        }
        else
        {
            _logger.LogInformation("Using DefaultAzureCredential");
            return new DefaultAzureCredential();
        }
    } 
}