namespace CineLingo.Services.Interfaces;

public interface ITranslationService
{
    /// <summary>
    /// Translates <paramref name="text"/> to <paramref name="targetLanguage"/>.
    /// </summary>
    /// <param name="text">Source text.</param>
    /// <param name="fromLanguage">
    /// BCP-47 tag as returned by the Speech SDK (e.g. "en-US", "zh-CN").
    /// Pass <c>null</c> to let the Translator service auto-detect.
    /// </param>
    /// <param name="targetLanguage">Translator language code (e.g. "zh-Hans", "en", "ja").</param>
    Task<string?> TranslateAsync(string text, string? fromLanguage, string targetLanguage,
        CancellationToken ct = default);
}
