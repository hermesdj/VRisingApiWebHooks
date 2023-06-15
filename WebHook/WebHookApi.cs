#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VRisingApiWebHooks.WebHook;

[JsonConverter(typeof(WebHookStatusConverter))]
public enum WebHookStatus
{
    Enabled,
    Disabled,
    Unknown
}

public class WebHookStatusConverter : JsonConverter<WebHookStatus>
{
    public override WebHookStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value != null ? Enum.Parse<WebHookStatus>(value) : WebHookStatus.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, WebHookStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Enum.GetName(typeof(WebHookStatus), value));
    }
}

public class WebHookEndpoint
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public long Created { get; set; }
    public List<string> EnabledEvents { get; set; } = new();
    public WebHookStatus Status { get; set; }
    public string? Url { get; set; }
    public string? Secret { get; set; }
}

public record struct CreateWebHookBody(
    string? Description,
    string Url,
    List<string> EnabledEvents
);

public record struct UpdateWebHookBody(
    string? Description,
    List<string>? EnabledEvents,
    string? Url,
    bool? Disable
);

public record struct DeleteWebHookResponse(
    Guid Id,
    bool Deleted
);

public record struct ListAllWebHooksResponse(
    List<WebHookEndpoint> Endpoints
);

public record struct ListAvailableEvents(
    List<string> AvailableEvents
);