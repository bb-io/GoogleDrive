using Blackbird.Applications.Sdk.Common.Exceptions;
using Google;
using System;

namespace Apps.GoogleDrive.Utils;

public static class ErrorHandler
{
    public static async Task ExecuteWithErrorHandlingAsync(this Func<Task> action)
    {
        try
        { 
            await action();
        }
        catch (GoogleApiException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }
    
    public static async Task<T> ExecuteWithErrorHandlingAsync<T>(this Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (GoogleApiException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }

    public static void ExecuteWithErrorHandling(Action action)
    {
        try
        {
            action();
        }
        catch (GoogleApiException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }

    public static T ExecuteWithErrorHandling<T>(this Func<T> action)
    {
        try
        {
            return action();
        }
        catch (GoogleApiException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }
}