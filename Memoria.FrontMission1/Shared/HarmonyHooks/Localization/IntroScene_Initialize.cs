using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using UnityEngine.Video;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(IntroScene), "Initialize")]
public static class IntroScene_Initialize
{
    public static void Prefix(VideoPlayer ___m_IntroMovie)
    {
        try
        {
            ModComponent.Instance.VideoControl.Initialize(___m_IntroMovie);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}