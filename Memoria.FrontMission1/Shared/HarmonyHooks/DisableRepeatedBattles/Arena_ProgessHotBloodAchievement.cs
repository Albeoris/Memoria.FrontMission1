using System;
using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
// ReSharper disable once StringLiteralTypo
// ReSharper disable once IdentifierTypo
[HarmonyPatch(typeof(Arena), "ProgessHotBloodAchievement")]
public static class Arena_ProgessHotBloodAchievement
{
    public static void Postfix(Arena __instance, Boolean ___countsForAchievement)
    {
        if (!___countsForAchievement)
            return;

        Int32 city = Session.Instance.MissionInfo.GetCityNo();
        ModComponent.Instance.ArenaWins.RememberWin(city, __instance.Wanzer_1, __instance.Wanzer_2);
    }
}