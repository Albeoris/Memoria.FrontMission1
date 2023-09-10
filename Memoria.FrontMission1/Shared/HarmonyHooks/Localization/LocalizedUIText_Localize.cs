using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Memoria.FrontMission1.Configuration;
using TMPro;
using UnityEngine;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(LocalizedUIText), "Localize")]
public static class LocalizedUIText_Localize
{
    public static void Prefix(LocalizedUIText __instance)
    {
        try
        {
            AssetsConfiguration config = ModComponent.Instance.Config.Assets;
            if (!config.ModsEnabled)
                return;

            LocalizedMessages.MessageInfo message = Localization.current.GetMessageInfo(__instance.Label);
            if (message is null)
                return;

            while (message.m_Value.StartsWith('$'))
            {
                String linkedKey = message.m_Value.Substring(1);
                message = Localization.current.GetMessageInfo(linkedKey);
                if (message != null)
                    __instance.Label = linkedKey;
                else
                    break;
            }

            if (__instance.DontLocalize)
            {
                if (Localization.current.GetMessageInfo(__instance.Label) != null)
                    __instance.DontLocalize = false;
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}

