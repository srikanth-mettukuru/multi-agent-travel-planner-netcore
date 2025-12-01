using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace MultiAgentTravelPlanner.Web.Utilities;

public static class AgentHelper
{
    /// <summary>
    /// Sends a chat completion request to OpenAI and returns the cleaned response text.
    /// </summary>
    public static async Task<string> GetChatCompletionAsync(
        ChatClient chatClient,
        string systemPrompt,
        string userPrompt,
        ILogger logger,
        string agentName)
    {
        logger.LogDebug("{AgentName}: Sending request to GPT", agentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await chatClient.CompleteChatAsync(messages);
        var content = completion.Value.Content[0].Text;

        logger.LogDebug("{AgentName} GPT Response: {Response}", agentName, content);

        // Clean up response (remove markdown if present)
        return CleanJsonResponse(content);
    }

    /// <summary>
    /// Removes markdown code block formatting from JSON responses.
    /// </summary>
    public static string CleanJsonResponse(string content)
    {
        content = content.Trim();
        
        if (content.StartsWith("```json"))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }
        
        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }
        
        return content.Trim();
    }

    /// <summary>
    /// Deserializes JSON content into the specified type.
    /// </summary>
    public static T? DeserializeJson<T>(string jsonContent) where T : class
    {
        return JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Creates a ChatClient with the provided OpenAI settings.
    /// </summary>
    public static ChatClient CreateChatClient(string apiKey, string model)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is required in configuration.");
        }

        return new ChatClient(model, new ApiKeyCredential(apiKey));
    }
}