using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using TMPro;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIMachineStatusPanel), "Initialize")]
public class UIMachineStatusPanel_Initialize
{
    private static readonly Single fontSize = 20;

    private static void Postfix(UIMachineStatusPanel __instance, ref TextMeshProUGUI ___m_MoveStat)
    {
        try
        {
            TMProResizer.Resize(___m_MoveStat.transform.parent.gameObject, fontSize);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}