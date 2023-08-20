using System;
using HarmonyLib;
using Memoria.FrontMission1.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIMainMenu), "Open")]
internal static class UIMainMenu_Open
{
    private static Canvas _canvas;

    private static void Postfix(UIMainMenu __instance)
    {
        try
        {
            if (!ModComponent.Instance.Config.Debug.EnableDebugMenu)
                return;

            _canvas = UIBuilder.CreateCanvas("Debug Menu", parent: null);

            GameObject hiddenPanel = CreatePanel("HiddenPanel", _canvas.gameObject);
            
            AddButton("Open Scene List", hiddenPanel, ShowSceneDebug);

            TMPButton debugButton = AddButton("Show / Hide", _canvas.gameObject,
                () => { hiddenPanel.SetActive(!hiddenPanel.activeSelf); }
            );

            RectTransform rect = debugButton.Image.rectTransform;
            rect.anchorMin = new Vector2(0.94f, 0.97f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10, 5);
            rect.offsetMax = new Vector2(-10, -5);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogError(ex);
        }
    }

    private static void ShowSceneDebug()
    {
        LoadingScreen.Instance.LoadScenarioTestingScene();
    }

    private static GameObject CreatePanel(String name, GameObject parent)
    {
        GameObject newPanel = UIBuilder.CreateUiObject(name, parent);

        Image panelImage = newPanel.AddComponent<Image>();
        Color panelColor = Color.black;
        panelColor.a = 0.3f;
        panelImage.color = panelColor;

        HorizontalLayoutGroup lGroup = newPanel.AddComponent<HorizontalLayoutGroup>();
        lGroup.padding = new RectOffset(20, 20, 40, 0);
        lGroup.childAlignment = TextAnchor.UpperCenter;
        lGroup.childControlHeight = false;

        RectTransform rect = panelImage.rectTransform;
        rect.anchorMin = new Vector2(0.85f, 0);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        newPanel.SetActive(false);
        return newPanel;
    }

    private static TMPButton AddButton(String name, GameObject parent, Action callback)
    {
        TMPButton button = UIBuilder.CreateButton(name, parent);
        button.Text = name;
        button.Click += callback;
        return button;
    }
}