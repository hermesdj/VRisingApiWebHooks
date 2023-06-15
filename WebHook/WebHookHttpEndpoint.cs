using Il2CppSystem;
using VRisingServerApiPlugin.attributes;
using VRisingServerApiPlugin.attributes.methods;
using VRisingServerApiPlugin.attributes.parameters;
using VRisingServerApiPlugin.http;
using VRisingServerEvents.Events;

namespace VRisingApiWebHooks.WebHook;

[HttpHandler("/hooks", allRouteProtected: true)]
public class WebHookHttpEndpoint
{
    [HttpGet("")]
    public ListAllWebHooksResponse GetAllEndpoints()
    {
        return new ListAllWebHooksResponse(Endpoints: WebHookConfig.Instance.Endpoints);
    }

    [HttpPost("")]
    public WebHookEndpoint CreateEndpoint([RequestBody] CreateWebHookBody body)
    {
        return WebHookConfig.Instance.CreateEndpoint(body.Url, body.Description, body.EnabledEvents);
    }

    [HttpPost(@"/{id}")]
    public WebHookEndpoint UpdateEndpoint([UrlParam("id")] Guid id, [RequestBody] UpdateWebHookBody body)
    {
        var endpoint =
            WebHookConfig.Instance.UpdateEndpoint(System.Guid.Parse(id.ToString()), body.Url, body.Description,
                body.EnabledEvents, body.Disable);

        if (endpoint == null)
        {
            throw new HttpException(404, $"The endpoint with id {id} does not exists !");
        }

        return endpoint;
    }

    [HttpGet(@"/{id}")]
    public WebHookEndpoint GetEndpoint([UrlParam("id")] Guid id)
    {
        var endpoint = WebHookConfig.Instance.GetEndpoint(System.Guid.Parse(id.ToString()));

        if (endpoint == null)
        {
            throw new HttpException(404, $"The endpoint with id {id} does not exists !");
        }

        return endpoint;
    }

    [HttpDelete(
        pattern: @"/{id}")]
    public DeleteWebHookResponse DeleteEndpoint([UrlParam("id")] Guid id)
    {
        return new DeleteWebHookResponse
        {
            Id = System.Guid.Parse(id.ToString()),
            Deleted = WebHookConfig.Instance.DeleteEndpoint(System.Guid.Parse(id.ToString()))
        };
    }

    [HttpGet("/available-events")]
    public ListAvailableEvents AvailableEvents()
    {
        return new ListAvailableEvents(AvailableEvents: EventManager.GetAllEvents());
    }
}