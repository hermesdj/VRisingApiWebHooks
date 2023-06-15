using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace VRisingApiWebHooks.WebHook;

public static class WebHookDispatcher
{
    public static IEnumerable<UnityWebRequestAsyncOperation> Dispatch(WebHookEndpoint endpoint, string tmp)
    {
        var request = new UnityWebRequest(endpoint.Url, "POST");
        request.SetUploadHandler(new UploadHandlerRaw(Encoding.UTF8.GetBytes(tmp)));
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
    }
}