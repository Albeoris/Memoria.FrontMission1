using System;
using System.IO;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;
using Walker.Data;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(BodyParts), "GetPart")]
public static class BodyParts_GetPart
{
    public static void Postfix(BodyPartData __result)
    {
        try
        {
            if (__result.m_HasLocalizedName)
                return;
            
            String helpMessage = __result.m_HelpMessage.Localize();
            foreach ((Char startChar, Char endChar) in new[] { ('«', '»'), ('"', '"'), ('\'', '\'') })
            {
                if (TryFindTitle(ref __result.m_PartName, helpMessage, startChar, endChar))
                {
                    __result.m_HasLocalizedName = true;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }

    private static Boolean TryFindTitle(ref String __result, String helpMessage, Char startChar, Char endChar)
    {
        Int32 firstQuote = helpMessage.IndexOf(startChar);
        if (firstQuote < 0)
            return false;

        Int32 secondQuote = helpMessage.IndexOf(endChar, firstQuote + 1);
        if (secondQuote < 0)
            return false;

        __result = helpMessage.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
        return true;
    }
}