using System.Text.Json.Serialization;

namespace ClaudeGit;

// Anthropic Messages API
record ApiMessage(string Role, string Content);
record ApiRequest(string Model, int MaxTokens, ApiMessage[] Messages);
record ApiContent(string Type, string Text);
record ApiError(string Type, string Message);
record ApiResponse(ApiContent[]? Content, ApiError? Error);

// GitHub Releases API
record GitHubRelease(string TagName, GitHubAsset[] Assets);
record GitHubAsset(string Name, string BrowserDownloadUrl);

[JsonSerializable(typeof(ApiRequest))]
[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal partial class AppJsonContext : JsonSerializerContext { }
