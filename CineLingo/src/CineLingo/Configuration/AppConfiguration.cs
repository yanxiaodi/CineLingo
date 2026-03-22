namespace CineLingo.Configuration;

public static class AppConfiguration
{
    // Azure AI Speech resource key.
    // Azure portal → your Speech resource → Keys and Endpoint → Key 1
    public const string SpeechSubscriptionKey = "YOUR_AZURE_SPEECH_KEY";

    // Endpoint URL from Azure portal → Keys and Endpoint → Endpoint
    // e.g. "https://funcoding.cognitiveservices.azure.com/"
    public const string SpeechEndpoint = "https://funcoding.cognitiveservices.azure.com/";
}