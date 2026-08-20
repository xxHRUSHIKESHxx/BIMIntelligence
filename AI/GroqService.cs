using BIMIntelligence.AI.Models;
using BIMIntelligence.Tools;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
namespace BIMIntelligence.AI;

public class GroqService
{
    private readonly HttpClient _httpClient;

    private const string GroqEndpoint =
        "https://api.groq.com/openai/v1/chat/completions";

    private const string Model =
        "llama-3.3-70b-versatile";


    public ToolCallRequest? ParseToolCall(string responseBody)
    {
        using JsonDocument document =
            JsonDocument.Parse(responseBody);

        JsonElement message =
            document
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message");

        // No tool call means Groq decided that
        // the question cannot/doesn't need to use a tool.
        if (!message.TryGetProperty(
                "tool_calls",
                out JsonElement toolCalls))
        {
            return null;
        }

        if (toolCalls.GetArrayLength() == 0)
        {
            return null;
        }

        JsonElement toolCall =
            toolCalls[0];

        JsonElement function =
            toolCall.GetProperty("function");

        string name =
            function
                .GetProperty("name")
                .GetString()
            ?? string.Empty;

        string arguments =
            function
                .GetProperty("arguments")
                .GetString()
            ?? "{}";

        return new ToolCallRequest
        {
            Name = name,
            ArgumentsJson = arguments
        };
    }
    public GroqService()
    {
        GroqLogger.Log("Creating GroqService.");

        string? apiKey =
            Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            GroqLogger.Log(
                "ERROR: GROQ_API_KEY is not configured.");

            throw new InvalidOperationException(
                "GROQ_API_KEY environment variable is not configured.");
        }

        var handler = new HttpClientHandler
        {
            UseProxy = false
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Store the key for use when creating the request.
        _apiKey = apiKey;

        GroqLogger.Log(
            "HttpClient configured.");
    }

    private readonly string _apiKey;

    public async Task<string> AskAsync(string prompt)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        GroqLogger.Log(
            $"Starting Groq request. Prompt: {prompt}");

        var requestBody = new
        {
            model = Model,

            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },

            temperature = 0.2
        };

        string json =
            JsonSerializer.Serialize(requestBody);

        GroqLogger.Log(
            $"Request JSON created. Length: {json.Length}");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                GroqEndpoint);

        request.Headers.Add(
            "Authorization",
            $"Bearer {_apiKey}");

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        GroqLogger.Log(
            "Authorization header configured.");

        GroqLogger.Log(
            "Sending HTTP POST request to Groq...");

        try
        {
            using HttpResponseMessage response =
                    await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false);

            stopwatch.Stop();

            GroqLogger.Log(
                $"HTTP response received. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");

            string responseBody =
                    await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

            GroqLogger.Log(
                $"Response body received. " +
                $"Length: {responseBody.Length}");

            if (!response.IsSuccessStatusCode)
            {
                GroqLogger.Log(
                    $"Groq API ERROR: {responseBody}");

                throw new HttpRequestException(
                    $"Groq API returned " +
                    $"{(int)response.StatusCode} " +
                    $"{response.StatusCode}: " +
                    responseBody);
            }

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            string result =
                document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    ?? string.Empty;

            GroqLogger.Log(
                $"Groq response parsed successfully. " +
                $"Total elapsed: " +
                $"{stopwatch.ElapsedMilliseconds} ms.");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            GroqLogger.Log(
                $"Groq request failed after " +
                $"{stopwatch.ElapsedMilliseconds} ms: {ex}");

            throw;
        }
    }

    public async Task<string> AskWithToolsAsync(string prompt)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        var requestBody = new
        {
            model = Model,

            messages = new object[]
            {
            new
            {
                role = "system",
                content =
                    "You are a Revit BIM assistant. " +
                        "Use the available tools to answer questions about the" +
                        "current Revit model."+
                        "Only use information returned by the tools.Never invent model information.If the available tools cannot answer the user question,clearly state that the requested information is not currently supported." ,
            },

            new
            {
                role = "user",
                content = prompt
            }
            },

            tools = ToolDefinitions.GetTools(),

            tool_choice = "auto",

            temperature = 0.1
        };

        string json =
            JsonSerializer.Serialize(requestBody);

        GroqLogger.Log(
            $"Tool request JSON length: {json.Length}");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                GroqEndpoint);

        request.Headers.Add(
            "Authorization",
            $"Bearer {_apiKey}");

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        GroqLogger.Log(
            "Sending tool-enabled request to Groq.");

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        string responseBody =
            await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

        GroqLogger.Log(
            $"Tool request response: {responseBody}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Groq API returned " +
                $"{(int)response.StatusCode}: " +
                responseBody);
        }

        ToolCallRequest? toolCall =
            ParseToolCall(responseBody);

        if (toolCall == null)
        {
            return "No tool call was requested.";
        }

        return responseBody;
    }

    public string AskForFinalResponse(
    string originalQuestion,
    string toolName,
    string toolCallId,
    string toolResult)
    {
        var messages = new object[]
        {
        new
        {
            role = "system",
            content =
                "You are a Revit BIM assistant. " +
                "Answer questions using only the provided Revit data. " +
                "Do not invent values. " +
                "Give concise, natural-language answers."
        },

        new
        {
            role = "user",
            content = originalQuestion
        },

        new
        {
            role = "assistant",

            tool_calls = new[]
            {
                new
                {
                    id = toolCallId,
                    type = "function",

                    function = new
                    {
                        name = toolName,
                        arguments = "{}"
                    }
                }
            }
        },

        new
        {
            role = "tool",
            tool_call_id = toolCallId,
            name = toolName,
            content = toolResult
        }
        };

        return SendChatRequest(
                messages,
                includeTools: false)
            .GetAwaiter()
            .GetResult();
    }


    private async Task<string> SendChatRequest(
    object[] messages,
    bool includeTools)
    {
        var requestBody = new
        {
            model = Model,

            messages,

            tools = includeTools
                ? ToolDefinitions.GetTools()
                : null,

            tool_choice = includeTools
                ? "auto"
                : "none",

            temperature = 0.1
        };

        string json =
            JsonSerializer.Serialize(requestBody);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                GroqEndpoint);

        request.Headers.Add(
            "Authorization",
            $"Bearer {_apiKey}");

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        GroqLogger.Log(
            $"Sending Groq request. Tools enabled: {includeTools}");

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        string responseBody =
            await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Groq API returned " +
                $"{(int)response.StatusCode}: " +
                responseBody);
        }

        GroqLogger.Log(
            $"Groq response received: {responseBody}");

        return responseBody;
    }

    public string ParseFinalResponse(string responseBody)
    {
        using JsonDocument document =
            JsonDocument.Parse(responseBody);

        JsonElement message =
            document
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message");

        if (!message.TryGetProperty(
                "content",
                out JsonElement content))
        {
            return "I couldn't generate a response.";
        }

        return content.GetString()
            ?? "I couldn't generate a response.";
    }
    public string GetFinalResponse(
    string question,
    ToolCallRequest toolCall,
    ToolResult toolResult)
    {
        var messages = new object[]
        {
        new
        {
            role = "system",
            content =
                "You are a Revit BIM assistant. " +
                "Answer using only the provided Revit tool results. " +
                "Never invent model information. " +
                "Respond clearly and concisely."
        },

        new
        {
            role = "user",
            content = question
        },

        new
        {
            role = "assistant",

            tool_calls = new[]
            {
                new
                {
                    id = toolCall.Id,

                    type = "function",

                    function = new
                    {
                        name = toolCall.Name,

                        arguments =
                            toolCall.ArgumentsJson
                    }
                }
            }
        },

        new
        {
            role = "tool",

            tool_call_id = toolCall.Id,

            name = toolCall.Name,

            content = toolResult.Success
                ? toolResult.Data
                : JsonSerializer.Serialize(
                    new
                    {
                        error = toolResult.Error,
                        is_error = true
                    })
        }
        };

        string response =
            SendChatRequest(
                    messages,
                    includeTools: false)
                .GetAwaiter()
                .GetResult();

        using JsonDocument document =
            JsonDocument.Parse(response);

        return document
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()
            ?? string.Empty;
    }


}