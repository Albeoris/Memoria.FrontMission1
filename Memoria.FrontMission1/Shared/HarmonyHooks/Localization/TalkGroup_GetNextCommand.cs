using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(TalkGroup), "GetNextCommand")]
public static class TalkGroup_GetNextCommand
{
    private static Int32 LastTalkGroupHash { get; set; }
    private static TALK_MSG PreviousTalkCommand { get; set; }
    private static TALK_MSG LastTalkCommand { get; set; }

    public static Boolean RepeatPrevious { get; set; }
    public static Boolean RepeatLast { get; set; }
    public static String LastMessageId => LastTalkCommand?.MessageID;

    public static Boolean Prefix(TalkGroup __instance, out TalkCmd __result,  out Boolean __state)
    {
        __state = false;
        __result = default;
        
        try
        {
            if (__instance.GetHashCode() != LastTalkGroupHash)
            {
                PreviousTalkCommand = null;
                LastTalkCommand = null;
                return true; // call original
            }

            if (RepeatPrevious)
            {
                RepeatPrevious = false;
                RepeatLast = false;
                if (PreviousTalkCommand != null)
                {
                    __result = PreviousTalkCommand;
                    __state = true;
                    return false; // don't call original
                }

                if (LastTalkCommand != null)
                {
                    __result = LastTalkCommand;
                    __state = true;
                    return false; // don't call original
                }
            }
        
            if (RepeatLast)
            {
                RepeatLast = false;
                if (LastTalkCommand != null)
                {
                    __result = LastTalkCommand;
                    __state = true;
                    return false; // don't call original
                }
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
        
        return true; // call original
    }

    public static void Postfix(TalkGroup __instance, ref TalkCmd __result, Boolean __state)
    {
        try
        {
            if (__state)
                return;

            if (__result.Command != TalkCommand.MSG)
                return;

            Int32 newHash = __instance.GetHashCode();
            if (LastTalkGroupHash == newHash)
            {
                PreviousTalkCommand = LastTalkCommand;
            }
            else
            {
                PreviousTalkCommand = null;
                LastTalkGroupHash = newHash;
            }

            LastTalkCommand = __result as TALK_MSG;
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}