using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(UIArenaEnemySelectMenu), "Arena_get_city_bet")]
public static class UIArenaEnemySelectMenu_Arena_get_city_bet
{
    public static void Postfix(UIArenaEnemySelectMenu __instance, Int32 num, Session ___m_Session, ref Int32 __result)
    {
        try
        {
            if (!ModComponent.Instance.Config.Arena.DisableRepeatedBattles)
                return;

            Int32 betMask = __result;
            __result = GetGreatest(betMask);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }

    private static Int32 GetGreatest(Int32 betMask)
    {
        const Int32 cancel = (Int32)BetId.BetId_Cancel;

        Int32 result = 0;
        if ((betMask & cancel) == cancel)
        {
            result = cancel;
            betMask &= ~cancel;
        }

        Int32 counter = 0;
        while (betMask > 0)
        {
            counter++;
            betMask >>= 1;
        }

        if (counter > 0)
            result |= 1 << (counter - 1);

        return result;
    }

    private enum BetId
    {
        BetId_NoBet     = 0b0000_0000_0001,
        BetId_OneCoin   = 0b0000_0000_0010,
        BetId_Mini      = 0b0000_0000_0100,
        BetId_Small     = 0b0000_0000_1000,
        BetId_Middle    = 0b0000_0001_0000,
        BetId_Big       = 0b0000_0010_0000,
        BetId_Highlevel = 0b0000_0100_0000,
        BetId_Bronze    = 0b0000_1000_0000,
        BetId_Silver    = 0b0001_0000_0000,
        BetId_Gold      = 0b0010_0000_0000,
        BetId_Royal     = 0b0100_0000_0000,
        BetId_Cancel    = 0b1000_0000_0000
    }
}