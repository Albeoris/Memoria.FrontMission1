using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(Session), "SetDefaultName")]
public static class Session_SetDefaultName
{
    public static void Postfix(Session __instance)
    {
        try
        {
            String key = __instance.GetGameMode() == GAME_MODE.GAME_MODE_ANOTHER
                ? "Memoria.Hardcoded.DefaultName.Kevin"
                : "Memoria.Hardcoded.DefaultName.Royd";

            if (LocalizationHelper.TryGet(key, out String name))
            {
                ModComponent.Log.LogMessage($"Default pilot name changed to {name}");
                __instance.Prefs.SetPilotName(0, name);
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}