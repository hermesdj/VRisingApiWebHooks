#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BepInEx;
using Il2CppSystem.IO;
using VRisingServerApiPlugin.http;
using VRisingServerEvents.Events;

namespace VRisingApiWebHooks.WebHook;

/// <summary>
/// Class used to store the webhook config on the file system and provide method to CRUD the endpoints
/// </summary>
public class WebHookConfig
{
    public List<WebHookEndpoint> Endpoints { get; set; } = new();
    public static WebHookConfig Instance = new();

    private static string ConfigPath { get; set; } =
        Utility.CombinePaths(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}-webhooks.json");

    /// <summary>
    /// Mandatory for JSON serialization/deserialization to work
    /// </summary>
    public WebHookConfig()
    {
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.Strict,
        WriteIndented = true
    };

    /// <summary>
    /// Reload the config from the file system
    /// </summary>
    private static void Reload()
    {
        var jsonText = File.ReadAllText(ConfigPath);
        if (jsonText == null) return;

        var deserialized = JsonSerializer.Deserialize<WebHookConfig>(jsonText, SerializerOptions);

        if (deserialized != null)
        {
            Instance = deserialized;
        }
    }

    /// <summary>
    /// Write the WebHook config to a JSON file
    /// </summary>
    private static void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Instance, typeof(WebHookConfig), SerializerOptions));
    }

    /// <summary>
    /// Initialize the WebHookConfig. If the config .json file exists on the filepath, will load the config into memory
    /// </summary>
    public static void Initialize()
    {
        if (File.Exists(ConfigPath))
        {
            Reload();
        }
    }

    /// <summary>
    /// Create a new WebHook endpoint that will be called by the API
    /// </summary>
    /// <param name="url">The URL of the endpoint to POST messages to</param>
    /// <param name="description">The description of the endpoint</param>
    /// <param name="enabledEvents">A list of event names or event name patterns.</param>
    /// <returns></returns>
    public WebHookEndpoint CreateEndpoint(string url, string? description, List<string> enabledEvents)
    {
        var hmac = new HMACSHA256();

        CheckEventsProvided(enabledEvents);

        var endpoint = new WebHookEndpoint
        {
            Id = Guid.NewGuid(),
            Description = description,
            Url = url,
            Created = DateTime.Now.ToFileTimeUtc(),
            EnabledEvents = enabledEvents,
            Status = WebHookStatus.Enabled,
            Secret = $"vrsec_{Convert.ToHexString(hmac.Key)}"
        };

        Endpoints.Add(endpoint);

        Save();

        return endpoint;
    }

    /// <summary>
    /// Check that a list of events provided (can be full event name, or event regex pattern) match at least one of the available events
    /// </summary>
    /// <param name="eventsOrPatterns">The list of events to check</param>
    /// <exception cref="HttpException">Throws a 400 Bad Request error if it does not match</exception>
    private static void CheckEventsProvided(IEnumerable<string> eventsOrPatterns)
    {
        var availableEventList = EventManager.GetAllEvents();

        var allPatternsMatch =
            eventsOrPatterns.All(pattern => availableEventList.Any(ev => Regex.IsMatch(ev, pattern)));

        if (!allPatternsMatch)
        {
            throw new HttpException(400,
                $"Event list provided is invalid. Please check allowed event list or pattern provided. At least one event should match.");
        }
    }

    /// <summary>
    /// Retrieve an Endpoint using its ID
    /// </summary>
    /// <param name="id">The ID of the endpoint</param>
    /// <returns>The endpoint or null if no endpoint exists for this id</returns>
    public WebHookEndpoint? GetEndpoint(Guid id)
    {
        if (Endpoints.Count == 0)
        {
            Reload();
        }

        return Endpoints.FirstOrDefault(endpoint => endpoint.Id.Equals(id));
    }

    /// <summary>
    /// Update an endpoint
    /// </summary>
    /// <param name="id">The id of the endpoint to update</param>
    /// <param name="url">A new URL or null</param>
    /// <param name="description">A new Description or null</param>
    /// <param name="enabledEvents">A list of events to subscribe to. Will throw an error if it is not valid</param>
    /// <param name="disable">Disable the endpoint</param>
    /// <returns>The updated endpoint</returns>
    public WebHookEndpoint? UpdateEndpoint(Guid id, string? url, string? description,
        List<string>? enabledEvents,
        bool? disable)
    {
        var endpoint = GetEndpoint(id);

        if (endpoint == null) return null;

        if (description != null)
        {
            endpoint.Description = description;
        }

        if (url != null)
        {
            endpoint.Url = url;
        }

        if (enabledEvents != null)
        {
            CheckEventsProvided(enabledEvents);
            endpoint.EnabledEvents = enabledEvents;
        }

        if (disable != null)
        {
            endpoint.Status = disable.Value ? WebHookStatus.Disabled : WebHookStatus.Enabled;
        }

        Save();

        return endpoint;
    }

    /// <summary>
    /// Delete the endpoint
    /// </summary>
    /// <param name="id">the id of the endpoint to delete</param>
    /// <returns>The id of the endpoint and a boolean if it has been deleted</returns>
    public bool DeleteEndpoint(Guid id)
    {
        var endpoint = GetEndpoint(id);

        if (endpoint == null) return false;

        var deleted = Endpoints.Remove(endpoint);
        Save();

        return deleted;
    }
}