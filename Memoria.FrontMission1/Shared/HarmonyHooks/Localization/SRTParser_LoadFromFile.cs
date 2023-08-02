using System;
using System.Collections.Generic;
using System.Linq;
using FE;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using UnityEngine;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(SRTParser), "LoadFromFile")]
public static class SRTParser_LoadFromFile
{
    public static Boolean Prefix(ref String fName)
    {
        try
        {
            if (!ModComponent.Instance.Config.Assets.ModsEnabled)
                return true; // call original

            String partialPath = $"Localization/{Localization.language}/{fName.Substring(Application.streamingAssetsPath.Length + 1)}";
            IReadOnlyList<String> modFiles = ModComponent.Instance.ModFiles.FindAll(partialPath);

            if (modFiles.Count != 0)
            {
                fName = modFiles.Last();
                ModComponent.Log.LogMessage($"[{nameof(SRTParser_LoadFromFile)}] Applied: {partialPath} localized file.");
            }

            return true; // call original
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }

        return true; // call original
    }
}