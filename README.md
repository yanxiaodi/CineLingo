# CineLingo

A .NET 10 MAUI app that provides **real-time AI-powered captions and translation** while you watch foreign-language movies. Point your phone at the TV, press Start — CineLingo listens to the room audio, recognises speech, and scrolls captions on screen continuously.

---

## Features

### 🎙️ Smart Speech Recognition
- **Continuous listening** — no button mashing; captions scroll automatically until you press Stop
- **Multi-language auto-detection** — detects up to 4 languages simultaneously on first utterance
- **English enhanced mode** — when English is detected, automatically switches to Azure Conversation Transcription for richer punctuation and speaker diarization
- **Speaker diarization** — colour-coded captions per speaker (Guest 1 = white, Guest 2 = yellow, Guest 3 = cyan…)

### 🌐 Real-Time Translation
- Translates each finalised caption via Azure Translator Text API v3
- Translated text shown as the **primary** caption; original shown below as a small subtitle
- Supports 12 target languages: Simplified Chinese, Traditional Chinese, English, Japanese, Korean, French, German, Spanish, Portuguese, Russian, Italian, Arabic
- Toggle translation on/off with the 🌐 button in the toolbar

### ⚙️ Settings
| Setting | Default | Description |
|---|---|---|
| Enable Translation | On | Toggle real-time translation |
| Target Language | 简体中文 | Language to translate into |
| Caption Font Size | 18 | Slider 12–30, live preview |
| Max Captions Shown | 100 | Slider 0–500; 0 = Unlimited |
| Speaker Diarization | On | Colour-code speakers in English mode |

### 🎨 UI
- Full-screen dark (black) theme optimised for ambient viewing
- Compact bottom toolbar: **[CL logo] [🌐 Translate] [⚙️ Settings] [Start/Stop] [● Listening]**
- Pulsing listening indicator while recognition is active
- Status overlay (language detection progress) shown inside the button

---

## Requirements

- .NET 10 + MAUI workload
- Android device (or emulator with microphone)
- **Azure AI Speech** resource (Standard S0) — for speech recognition and conversation transcription
- **Azure Translator** resource — for real-time translation

---

## Setup

### 1. Create Azure resources

| Resource | Tier | Used for |
|---|---|---|
| Azure AI Speech | Standard S0 | Continuous recognition + conversation transcription |
| Azure Translator | Free F0 | Text translation |

### 2. Configure credentials

Create `CineLingo/src/CineLingo/Configuration/AppConfiguration.cs` (this file is `.gitignore`d — never commit it):

```csharp
namespace CineLingo.Configuration;

public static class AppConfiguration
{
    // Azure AI Speech (Standard S0)
    public const string SpeechSubscriptionKey = "<your-speech-key>";
    public const string SpeechRegion          = "<your-region>";   // e.g. "australiaeast"

    // Azure Translator
    public const string TranslatorKey      = "<your-translator-key>";
    public const string TranslatorRegion   = "<your-region>";
    public const string TranslatorEndpoint = "https://api.cognitive.microsofttranslator.com/";
}
```

### 3. Build and run

```bash
dotnet build -f net10.0-android
```

Or open `CineLingo.sln` in Visual Studio 2022 and deploy to your Android device.

---

## Architecture

```
CineLingo/
├── Configuration/
│   └── AppConfiguration.cs          # Azure credentials (gitignored)
├── Models/
│   ├── Caption.cs                   # ObservableObject: Text, TranslatedText, DisplayText, SpeakerColor
│   ├── AppSettings.cs               # All user preferences (Preferences-backed)
│   └── FinalResultEventArgs.cs      # Carries Text + SpeakerId + DetectedLanguage
├── Services/
│   ├── Interfaces/
│   │   ├── ISpeechCaptionService.cs # PartialResultReceived / FinalResultReceived / StatusChanged
│   │   └── ITranslationService.cs  # TranslateAsync(text, fromLang, toLang)
│   └── Implementations/
│       ├── AzureSpeechCaptionService.cs       # SpeechRecognizer, multi-language auto-detect
│       ├── AzureConversationCaptionService.cs # ConversationTranscriber, diarization
│       ├── SmartSpeechOrchestrator.cs         # Detects language → routes to correct service
│       └── AzureTranslatorService.cs          # REST client for Translator Text API v3
├── PageModels/
│   ├── MainPageModel.cs             # Captions, translation, toolbar commands
│   └── SettingsPageModel.cs        # Settings bindings, DynamicResource font update
├── MainPage.xaml[.cs]               # Captions list + bottom toolbar + listening animation
└── SettingsPage.xaml[.cs]           # Settings UI with sliders
```

### Dual-mode speech engine

```
Start
  └─► AzureSpeechCaptionService (multi-language, up to 4 langs)
        └─► First utterance detected
              ├─► English + diarization ON → switch to AzureConversationCaptionService
              │     └─► Speaker-tagged, punctuated captions
              └─► Other language → stay in multi-language mode
```

---

## Tech Stack

| Library | Version | Purpose |
|---|---|---|
| .NET MAUI | .NET 10 | Cross-platform UI |
| CommunityToolkit.Maui | 14.x | UI helpers |
| CommunityToolkit.Mvvm | 8.x | MVVM source generators |
| Microsoft.CognitiveServices.Speech | 1.41+ | Azure Speech SDK |
| Azure Translator Text API v3 | REST | Translation |
