using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(UIMainMenu), "Open")]
internal static class UIMainMenu_Open
{
    private static GameObject _canvasObject;

    private static void Postfix(UIMainMenu __instance)
    {
        try
        {
            if (!ModComponent.Instance.Config.Debug.EnableDebugMenu)
                return;
            
            CreateCanvas();

            GameObject hiddenPanel = CreatePanel("HiddenPanel", _canvasObject.transform);

            CreateButton("Open Scene List", hiddenPanel.transform, ShowSceneDebug);

            GameObject debugButton = CreateButton("Show / Hide", _canvasObject.transform,
                () => { hiddenPanel.SetActive(!hiddenPanel.activeSelf); }
            );

            RectTransform _textRectTransform = debugButton.GetComponent<Image>().rectTransform;
            _textRectTransform.anchorMin = new Vector2(0.94f, 0.97f);
            _textRectTransform.anchorMax = Vector2.one;
            _textRectTransform.offsetMin = new Vector2(10, 5);
            _textRectTransform.offsetMax = new Vector2(-10, -5);
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
    
    private static void CreateCanvas()
    {
        _canvasObject = new GameObject("CustomUI")
        {
            layer = LayerMask.NameToLayer("UI")
        };

        Canvas canvas = _canvasObject.AddComponent<Canvas>();
        _canvasObject.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
    }

    private static GameObject CreatePanel(string name, Transform parent, bool activeOnStart = false)
    {
        GameObject newPanel = new GameObject(name)
        {
            layer = LayerMask.NameToLayer("UI")
        };
        newPanel.transform.SetParent(parent, false);
        newPanel.transform.localPosition = new Vector3(0, 0, 0);

        Image panelImage = newPanel.AddComponent<Image>();
        Color panelColor = Color.black;
        panelColor.a = 0.3f;
        panelImage.color = panelColor;

        HorizontalLayoutGroup lGroup = newPanel.AddComponent<HorizontalLayoutGroup>();
        lGroup.padding = new RectOffset(20, 20, 40, 0);
        lGroup.childAlignment = TextAnchor.UpperCenter;
        lGroup.childControlHeight = false;

        RectTransform _textRectTransform = panelImage.rectTransform;
        _textRectTransform.anchorMin = new Vector2(0.85f, 0);
        _textRectTransform.anchorMax = Vector2.one;
        _textRectTransform.offsetMin = Vector2.zero;
        _textRectTransform.offsetMax = Vector2.zero;

        newPanel.SetActive(activeOnStart);

        return newPanel;
    }

    private static GameObject CreateButton(string name, Transform parent, Action callback)
    {
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject newButton = TMP_DefaultControls.CreateButton(resources);

        newButton.name = name;
        newButton.layer = LayerMask.NameToLayer("UI");
        newButton.transform.SetParent(parent, false);
        newButton.transform.localPosition = new Vector3(0, 0, 0);

        newButton.GetComponentInChildren<TextMeshProUGUI>().text = name;

        Button buttonScript = newButton.GetComponent<Button>();
        buttonScript.onClick.AddListener(delegate() { callback?.Invoke(); });

        return newButton;
    }
}