using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIBonusExpPanel), "Open")]
public class UIBonusExpPanel_Open
{
    private static readonly Single fontSize = 22;

    private static void Postfix(UIBonusExpPanel __instance, ref LocalizedUIText ___localizedTitleText)
    {
        try
        {
            if (___localizedTitleText.TextField.fontSize != fontSize)
            {
                TMProResizer.Resize(___localizedTitleText.transform.parent.gameObject, fontSize);
                ___localizedTitleText.TextField.fontSize = fontSize;
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}