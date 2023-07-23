using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(UIArenaEnemySelectMenu), "CreateBetOnValue")]
public static class UIArenaEnemySelectMenu_CreateBetOnValue
{
    public static void Postfix(UIArenaEnemySelectMenu __instance, Int32 value, Session ___m_Session)
    {
        try
        {
            if (!ModComponent.Instance.Config.Arena.DisableRepeatedBattles)
                return;

            ___m_Session.MissionInfo.AddMoney(value);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}