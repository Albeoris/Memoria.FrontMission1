using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using TMPro;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIRepairPanel), "Initialize")]
public class UIRepairPanel_Initialize
{
    private static readonly Single fontSize = 23;

    private static void Postfix(UIRepairPanel __instance, ref TextMeshProUGUI ___m_MoveValue)
    {
        try
        {
            TMProResizer.Resize(___m_MoveValue.transform.parent.gameObject, fontSize);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}