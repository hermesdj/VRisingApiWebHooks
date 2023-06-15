#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using Il2CppSystem.IO;

namespace VRisingApiWebHooks.WebHook;

public class WebHookConfig
{
    public List<WebHookEndpoint> Endpoints { get; set; } = new();
    public static WebHookConfig Instance = new();

    private static string ConfigPath { get; set; } =
        Utility.CombinePaths(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}-webhooks.json");

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

    private static void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Instance, typeof(WebHookConfig), SerializerOptions));
    }

    public static void Initialize()
    {
        if (File.Exists(ConfigPath))
        {
            Reload();
        }
    }

    public WebHookEndpoint CreateEndpoint(string url, string? description, List<string> enabledEvents)
    {
        var hmac = new HMACSHA256();

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

    public WebHookEndpoint? GetEndpoint(Guid id)
    {
        if (Endpoints.Count == 0)
        {
            Reload();
        }

        return Endpoints.FirstOrDefault(endpoint => endpoint.Id.Equals(id));
    }

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
            endpoint.EnabledEvents = enabledEvents;
        }

        if (disable != null)
        {
            endpoint.Status = disable.Value ? WebHookStatus.Disabled : WebHookStatus.Enabled;
        }

        Save();

        return endpoint;
    }

    public bool DeleteEndpoint(Guid id)
    {
        var endpoint = GetEndpoint(id);

        if (endpoint == null) return false;

        var deleted = Endpoints.Remove(endpoint);
        Save();

        return deleted;
    }
}