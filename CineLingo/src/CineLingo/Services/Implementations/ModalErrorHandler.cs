﻿using CineLingo.Services.Interfaces;
using CineLingo.Utilities;

namespace CineLingo.Services.Implementations;
/// <summary>
/// Modal Error Handler.
/// </summary>
public class ModalErrorHandler : IErrorHandler
{
    readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Handle error in UI.
    /// </summary>
    /// <param name="ex">Exception.</param>
    public void HandleError(Exception ex)
    {
        DisplayAlert(ex).FireAndForgetSafeAsync();
    }

    async Task DisplayAlert(Exception ex)
    {
        try
        {
            await _semaphore.WaitAsync();
            if (Shell.Current is { } shell)
                await shell.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
