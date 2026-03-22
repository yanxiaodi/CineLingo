# CineLingo – Copilot Instructions

## Project Overview

CineLingo is a .NET 10 MAUI cross-platform app (Android, iOS, macCatalyst, Windows) that performs real-time speech-to-text transcription using Azure Cognitive Services, displaying live captions on screen. Primary use case: watching foreign-language movies and reading live captions.

## Build Commands

```bash
# Build for a specific platform (from solution root)
dotnet build CineLingo/src/CineLingo/CineLingo.csproj -f net10.0-android
dotnet build CineLingo/src/CineLingo/CineLingo.csproj -f net10.0-windows10.0.19041.0

# Run on Windows
dotnet run --project CineLingo/src/CineLingo/CineLingo.csproj -f net10.0-windows10.0.19041.0
```

There are no automated tests in this project.

## Azure Speech Configuration

### ⚠️ CRITICAL — NEVER COMMIT AppConfiguration.cs ⚠️

`AppConfiguration.cs` is listed in `.gitignore` and must **never** be staged or committed.
It contains a real Azure API key. Committing it leaks credentials into the public repository and requires:
1. Immediately regenerating the key in Azure portal
2. Rewriting the entire git history with `git filter-repo`

Before running, fill in your Azure Speech credentials **locally** in:

```
CineLingo/src/CineLingo/Configuration/AppConfiguration.cs
```

```csharp
public const string SpeechSubscriptionKey = "YOUR_AZURE_SPEECH_KEY";
public const string SpeechEndpoint = "YOUR_ENDPOINT_URL";
```

This file is git-ignored. When the repo is freshly cloned, copy it from the template
(`AppConfiguration.cs.template` if one exists) or create it manually.

## Architecture

Single-page MAUI app following MVVM with CommunityToolkit source generators:

```
MauiProgram.cs              ← DI setup: registers services and page models
AppShell.xaml               ← Shell (flyout disabled; single Captions route)
App.xaml.cs                 ← Creates AppShell window

Configuration/              ← Build-time constants (AppConfiguration.cs)
Models/                     ← ObservableObject data models (Caption)
PageModels/                 ← ViewModels (MainPageModel) — NOT "ViewModels" folder
Services/
  Interfaces/               ← ISpeechCaptionService, IErrorHandler
  Implementations/          ← AzureSpeechCaptionService, ModalErrorHandler
Behaviors/                  ← ScrollToBottomBehavior (CollectionView)
Utilities/                  ← FireAndForgetSafeAsync extension
Platforms/                  ← Platform-specific code
```

### Speech Recognition Flow

`AzureSpeechCaptionService` uses `SpeechRecognizer.StartContinuousRecognitionAsync()` with auto-language detection across 8 languages. It runs a persistent session (no re-call loop) and fires two events:

- `PartialResultReceived` → `Recognizing` event (live preview, shown dimmed/italic in the UI)
- `FinalResultReceived` → `Recognized` event (committed to `Captions` list)
- `ErrorReceived` → `Canceled` event with error details

`MainPageModel` subscribes to these events and dispatches UI updates via `MainThread.BeginInvokeOnMainThread`.

User presses **Start Captions** → recognition runs indefinitely → **Stop Captions** to end.

## Key Conventions

### MVVM Source Generators (CommunityToolkit.Mvvm)

Classes must be `partial`. Use the **partial property** syntax (required for AOT/WinRT compatibility in .NET 10):

```csharp
public partial class MyPageModel : ObservableObject
{
    // ✅ .NET 10 style — partial property
    [ObservableProperty]
    public partial string SomeValue { get; set; } = string.Empty;

    // ❌ Old style — private field (causes MVVMTK0045 warning on .NET 10)
    // [ObservableProperty] private string _someValue = string.Empty;

    [RelayCommand]
    private async Task DoSomething() { ... }  // generates DoSomethingCommand
}
```

### Naming

- ViewModels live in `PageModels/` and are suffixed `PageModel`.
- Services: interfaces in `Services/Interfaces/`, implementations in `Services/Implementations/`.
- Behaviors live in `Behaviors/` and extend `Behavior<TView>`.

### Error Handling

- Fire-and-forget async uses `FireAndForgetSafeAsync(IErrorHandler?)` from `TaskUtilities`.
- UI errors go through `IErrorHandler` / `ModalErrorHandler` (`Shell.DisplayAlertAsync`).
- A `SemaphoreSlim(1,1)` guards concurrent alert display in `ModalErrorHandler`.

### DI Registration (MauiProgram.cs)

- `AzureSpeechCaptionService` registered as singleton implementing `ISpeechCaptionService`.
- Page models and pages registered as singletons.
- Services are constructor-injected.

### XAML

- `x:DataType` set on pages and data templates for compiled bindings.
- `CollectionView` (not `ListView` — deprecated in .NET 10) with `ItemsUpdatingScrollMode="KeepLastItemInView"` for live caption scroll.
- Dark theme: `BackgroundColor="Black"`, finalized captions `TextColor="WhiteSmoke"`, partial/interim `TextColor="#888888"` + `FontAttributes="Italic"`.

### Platform-Specific Notes

- **Android**: `MainActivity.cs` locks orientation to `ScreenOrientation.Landscape`.
- **Windows**: The default launch profile (`Properties/launchSettings.json`) targets "Windows Machine".

## Key Dependencies

| Package | Version | Purpose |
|---|---|---|
| `CommunityToolkit.Maui` | 14.0.1 | Toast alerts |
| `CommunityToolkit.Mvvm` | 8.4.1 | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `Microsoft.CognitiveServices.Speech` | 1.48.2 | Azure continuous speech recognition |
| `Microsoft.Maui.Controls` | 10.0.50 | Pinned explicitly (workload default is lower) |
| `Microsoft.Extensions.Logging.Debug` | 10.0.5 | Debug logging |
