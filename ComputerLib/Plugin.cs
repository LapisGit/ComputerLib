using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ComputerLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static Harmony harmony;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo("we're all just playing on our computers yeah!");

        harmony = new Harmony("tech.lapiscreates.ComputerLib");
        harmony.PatchAll();
    }
}