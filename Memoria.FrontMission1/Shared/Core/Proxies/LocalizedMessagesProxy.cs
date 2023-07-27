using System;
using System.Reflection;
using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

public static class LocalizedMessagesProxy
{
    private static readonly MethodInfo Localization_SetLanguage = AccessTools.Method(typeof(Localization), "SetLanguage");

    public static void SetLanguage(Localization.GameLanguage newLanguage, Boolean force, Boolean callEvent)
    {
        TalkGroup_GetNextCommand.RepeatLast = true;
        Localization_SetLanguage.Invoke(obj: null, new Object[] { newLanguage, force, callEvent });
    }
}