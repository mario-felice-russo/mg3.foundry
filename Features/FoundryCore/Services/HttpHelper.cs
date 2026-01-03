using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using mg3.foundry.Features.FoundryCore.Models;

namespace mg3.foundry.Features.FoundryCore.Services;

/// <summary>
/// Centralized HTTP helper for consistent error handling and logging.
/// </summary>
public static class HttpHelper
{
    /// <summary>
    /// Performs a GET request and deserializes the response.
    /// </summary>
    public static async Task<Result<T>> GetAsync<T>(HttpClient client, string endpoint)
    {
        try
        {
            Debug.WriteLine($"GET {endpoint}");

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return await HandleErrorResponseAsync<T>(response, endpoint);
            }

            var result = await response.Content.ReadFromJsonAsync<T>();

            if (result == null)
            {
                return Result<T>.Failure("Response deserialization returned null", $"Endpoint: {endpoint}");
            }

            Debug.WriteLine($"GET {endpoint} - SUCCESS");
            return Result<T>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"GET {endpoint} - NETWORK ERROR: {ex.Message}");
            return Result<T>.Failure("Network error", ex.Message);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"GET {endpoint} - JSON ERROR: {ex.Message}");
            return Result<T>.Failure("Failed to parse response", ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GET {endpoint} - ERROR: {ex.Message}");
            return Result<T>.Failure("Unexpected error", ex.Message);
        }
    }

    /// <summary>
    /// Performs a POST request with a body and deserializes the response.
    /// </summary>
    public static async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        HttpClient client,
        string endpoint,
        TRequest body)
    {
        try
        {
            Debug.WriteLine($"POST {endpoint}");

            // Log request per debug
            var jsonBody = JsonSerializer.Serialize(body);
            Debug.WriteLine($"Request body: {jsonBody}");

            var response = await client.PostAsJsonAsync(endpoint, body);

            if (!response.IsSuccessStatusCode)
            {
                return await HandleErrorResponseAsync<TResponse>(response, endpoint);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>();

            if (result == null)
            {
                return Result<TResponse>.Failure("Response deserialization returned null", $"Endpoint: {endpoint}");
            }

            Debug.WriteLine($"POST {endpoint} - SUCCESS");
            return Result<TResponse>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"POST {endpoint} - NETWORK ERROR: {ex.Message}");
            return Result<TResponse>.Failure("Network error", ex.Message);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"POST {endpoint} - JSON ERROR: {ex.Message}");
            return Result<TResponse>.Failure("Failed to parse response", ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"POST {endpoint} - ERROR: {ex.Message}");
            return Result<TResponse>.Failure("Unexpected error", ex.Message);
        }
    }

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    public static async Task<Result<bool>> DeleteAsync(HttpClient client, string endpoint)
    {
        try
        {
            Debug.WriteLine($"DELETE {endpoint}");

            var response = await client.DeleteAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await HandleErrorResponseAsync<bool>(response, endpoint);
                return error;
            }

            Debug.WriteLine($"DELETE {endpoint} - SUCCESS");
            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"DELETE {endpoint} - NETWORK ERROR: {ex.Message}");
            return Result<bool>.Failure("Network error", ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DELETE {endpoint} - ERROR: {ex.Message}");
            return Result<bool>.Failure("Unexpected error", ex.Message);
        }
    }

    /// <summary>
    /// Handles error responses by extracting meaningful error messages.
    /// </summary>
    private static async Task<Result<T>> HandleErrorResponseAsync<T>(HttpResponseMessage response, string endpoint)
    {
        var statusCode = (int)response.StatusCode;
        var errorBody = await response.Content.ReadAsStringAsync();

        Debug.WriteLine($"{endpoint} - ERROR {statusCode}: {errorBody}");

        // Try to parse structured error response
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            var root = doc.RootElement;

            string? message = null;
            string? details = null;

            // RFC 7807 Problem Details format
            if (root.TryGetProperty("title", out var title))
            {
                message = title.GetString();
                if (root.TryGetProperty("detail", out var detail))
                {
                    details = detail.GetString();
                }
            }
            // OpenAI error format
            else if (root.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object)
                {
                    if (error.TryGetProperty("message", out var errMsg))
                        message = errMsg.GetString();
                    if (error.TryGetProperty("type", out var errType))
                    {
                        var type = errType.GetString();
                        if (!string.IsNullOrEmpty(details))
                            details = $"{type}: {details}";
                        else
                            details = type;
                    }
                }
                else if (error.ValueKind == JsonValueKind.String)
                {
                    message = error.GetString();
                }
            }
            // Generic message field
            else if (root.TryGetProperty("message", out var msg))
            {
                message = msg.GetString();
            }

            // **NUOVA GESTIONE: Bad allocation error**
            if (message?.Contains("bad allocation", StringComparison.OrdinalIgnoreCase) == true ||
                details?.Contains("bad allocation", StringComparison.OrdinalIgnoreCase) == true ||
                errorBody.Contains("bad allocation", StringComparison.OrdinalIgnoreCase))
            {
                message = "Model memory allocation failed";
                details = "The selected model cannot be loaded due to insufficient memory or incompatible configuration. Try:\n" +
                         "• Selecting a smaller model\n" +
                         "• Reducing max_tokens parameter\n" +
                         "• Checking if the model is compatible with your hardware\n" +
                         "• Restarting the Foundry server";
            }
            // Enhance error message for f16 issues
            else if (details?.Contains("f16", StringComparison.OrdinalIgnoreCase) == true ||
                     details?.Contains("float16", StringComparison.OrdinalIgnoreCase) == true)
            {
                message = "Model incompatible with your hardware";
                details = "This model requires float16 (f16) support which is not available on your device. Try selecting a different model with CPU or DirectML runtime.";
            }
            // HTTP 500 generic enhancement
            else if (statusCode == 500 && string.IsNullOrEmpty(message))
            {
                message = "Server internal error";
                details = errorBody.Length > 500 ? errorBody.Substring(0, 500) + "..." : errorBody;
            }

            return Result<T>.Failure(
                message ?? $"HTTP {statusCode} error",
                details ?? errorBody,
                statusCode
            );
        }
        catch
        {
            // If parsing fails, check for bad allocation in raw body
            if (errorBody.Contains("bad allocation", StringComparison.OrdinalIgnoreCase))
            {
                return Result<T>.Failure(
                    "Model memory allocation failed",
                    "The model cannot be loaded. Try selecting a smaller model or reducing parameters.",
                    statusCode
                );
            }

            // Return raw error
            return Result<T>.Failure(
                $"HTTP {statusCode} error",
                errorBody,
                statusCode
            );
        }
    }
}