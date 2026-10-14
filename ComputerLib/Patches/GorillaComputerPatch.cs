using ComputerLib.Utils;
using GorillaNetworking;
using HarmonyLib;

namespace ComputerLib.Patches;

[HarmonyPatch(typeof(GorillaComputer))]
[HarmonyWrapSafe]
public class GorillaComputerPatch
{
    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePatch(GorillaComputer __instance)
    {
        ComputerUtils.Initialize();
    }

    [HarmonyPatch(nameof(GorillaComputer.UpdateScreen)), HarmonyPostfix]
    public static void UpdateScreenPatch(GorillaComputer __instance)
    {
        ComputerUtils.Tick(__instance);
    }

    [HarmonyPatch(nameof(GorillaComputer.PressButton)), HarmonyPrefix]
    public static bool PressButtonPatch(GorillaComputer __instance, GorillaKeyboardBindings buttonPressed)
    {
        if (!ComputerUtils.OwnsState(__instance.currentState))
        {
            return true;
        }

        if (buttonPressed == GorillaKeyboardBindings.up || buttonPressed == GorillaKeyboardBindings.down)
        {
            return true;
        }

        ComputerUtils.OnInput(__instance, buttonPressed);
        ComputerUtils.Tick(__instance);
        return false;
    }
}
