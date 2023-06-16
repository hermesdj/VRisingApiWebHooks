using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VRisingApiWebHooks.WebHook;
using VRisingServerApiPlugin.command;
using VRisingServerEvents.Events;

namespace VRisingApiWebHooks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("jays.VRisingServerApiPlugin")]
[BepInDependency("jays.VRisingServerEvents")]
public class WebHookPlugin : BasePlugin
{
    Harmony _harmony;

    public override void Load()
    {
        // Plugin startup logic
        WebHookConfig.Initialize();
        CommandRegistry.RegisterAll();
        EventPublisher.RegisterEventHandlers();
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}