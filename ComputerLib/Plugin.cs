using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ComputerLib;

[BepInPlugin("tech.lapiscreates.ComputerLib", "ComputerLib", "1.0.0")]
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