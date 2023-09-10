using System;
using TMPro;
using UnityEngine;

namespace Memoria.FrontMission1.HarmonyHooks;

internal static class TMProResizer
{
    public static void Resize(GameObject gameObject, Single fontSize)
    {
        foreach (TMP_Text text in gameObject.GetComponentsInChildren<TMP_Text>())
        {
            if (text.name == "Text" && text.fontSize != fontSize)
                text.fontSize = fontSize;
        }
    }
}