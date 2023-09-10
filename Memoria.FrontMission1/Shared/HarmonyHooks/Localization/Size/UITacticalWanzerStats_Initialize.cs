using System;
using System.Collections.Generic;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using TMPro;
using UnityEngine;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UITacticalWanzerStats), "Initialize")]
public class UITacticalWanzerStats_Initialize
{
    private static readonly Single fontSize = 20;

    private static void Postfix(UITacticalWanzerStats __instance, ref TextMeshProUGUI[] ___m_MeleeStat)
    {
        try
        {
            HashSet<GameObject> set = new HashSet<GameObject>();
            for (Int32 i = 0; i < ___m_MeleeStat.Length; i++)
            {
                GameObject gameObject = ___m_MeleeStat[i].transform.parent.parent.gameObject;
                if (!set.Add(gameObject))
                    continue;
                
                foreach (TextMeshProUGUI text in gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.name == "Title" && text.fontSize != fontSize)
                        text.fontSize = fontSize;
                }
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}