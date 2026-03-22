using CineLingo.Configuration;
using CineLingo.Services.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CineLingo.Services.Implementations;

/// <summary>
/// Calls the Azure Translator Text API v3 to translate caption text.
/// Uses a single persistent HttpClient (service is registered as singleton).
/// </summary>
public sealed class AzureTranslatorService : ITranslationService, IDisposable
{
    private const string ApiVersion = "3.0";
    private readonly HttpClient _http;

    public AzureTranslatorService()
    {
        _http = new HttpClient { BaseAddress = new Uri(AppConfiguration.TranslatorEndpoint) };
        _http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppConfiguration.TranslatorKey);
        if (!string.IsNullOrEmpty(AppConfiguration.TranslatorRegion))
            _http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", AppConfiguration.TranslatorRegion);
    }

    public async Task<string?> TranslateAsync(string text, string? fromLanguage, string targetLanguage,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var translatorFrom = ToTranslatorCode(fromLanguage);

        // Skip the API call if source and target are the same language family.
        if (translatorFrom != null &&
            string.Equals(PrimaryTag(translatorFrom), PrimaryTag(targetLanguage), StringComparison.OrdinalIgnoreCase))
            return null;

        var url = $"translate?api-version={ApiVersion}&to={Uri.EscapeDataString(targetLanguage)}" +
                  (translatorFrom != null ? $"&from={Uri.EscapeDataString(translatorFrom)}" : string.Empty);

        var body = JsonSerializer.Serialize(new[] { new { Text = text } });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        // Response shape: [ { "translations": [ { "text": "...", "to": "zh-Hans" } ] } ]
        return doc.RootElement[0]
                  .GetProperty("translations")[0]
                  .GetProperty("text")
                  .GetString();
    }

    /// <summary>
    /// Maps a Speech SDK BCP-47 tag (e.g. "en-US") to an Azure Translator language code.
    /// Returns <c>null</c> for unknown tags so the Translator auto-detects.
    /// </summary>
    private static string? ToTranslatorCode(string? speechLang) =>
        speechLang?.ToLowerInvariant() switch
        {
            null         => null,
            "en-us" or "en-gb" or "en-au" or "en-ca" => "en",
            "zh-cn"      => "zh-Hans",
            "zh-tw" or "zh-hk" => "zh-Hant",
            "ja-jp"      => "ja",
            "ko-kr"      => "ko",
            _            => speechLang.Split('-')[0]   // best-effort: "fr-FR" → "fr"
        };

    /// <summary>Returns the primary language subtag (before the first '-').</summary>
    private static string PrimaryTag(string lang) =>
        lang.Split('-')[0].ToLowerInvariant();

    public void Dispose() => _http.Dispose();
}
