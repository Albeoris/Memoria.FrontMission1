using System;
using System.Collections.Generic;
using HarmonyLib;
using Walker;
using Wanzer = Walker.Data.Wanzer;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(UIArenaEnemySelectMenu), "OpenWanzerList")]
public static class UIArenaEnemySelectMenu_OpenWanzerList
{
    public static void Prefix(UIArenaEnemySelectMenu __instance, Walker.Data.Wanzer playerUnit, UIArenaMenu ___m_ArenaMenu)
    {
        if (!ModComponent.Instance.Config.Arena.DisableRepeatedBattles)
            return;
        
        Int32 city = Session.Instance.MissionInfo.GetCityNo();
        List<Wanzer> wanzers = ___m_ArenaMenu.m_WanzerList;

        for (Int32 i = wanzers.Count - 1; i >= 0; i--)
        {
            Wanzer enemy = wanzers[i];
            if (ModComponent.Instance.ArenaWins.HasAlreadyWin(city, playerUnit, enemy))
                wanzers.RemoveAt(i);
        }
    }
}