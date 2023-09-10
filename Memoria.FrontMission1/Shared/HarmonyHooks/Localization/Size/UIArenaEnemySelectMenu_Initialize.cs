using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using UnityEngine;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIArenaEnemySelectMenu), "Initialize")]
public class UIArenaEnemySelectMenu_Initialize
{
    private static readonly Single fontSize = 20;

    private static void Postfix(UIArenaEnemySelectMenu __instance, ref LocalizedUIText ___LocalizedMele, ref CanvasGroup ___m_VehicleDataCanvas)
    {
        try
        {
            if (___LocalizedMele.TextField.fontSize != fontSize)
            {
                TMProResizer.Resize(___m_VehicleDataCanvas.gameObject, fontSize);
                ___LocalizedMele.TextField.fontSize = fontSize;
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}