using System;
using System.Globalization;
using System.IO;
using FE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Walker;
using Object = System.Object;
using Path = System.IO.Path;

namespace Memoria.FrontMission1.Core;

public sealed class GameVideoControl : SafeComponent
{
    private VideoPlayer _videoPlayer;
    private SRTParser _srtParser;
    private SubtitleBlock _current;

    private GameObject _canvasObject;
    private ITextPresenter _subtitlesText;
    private ITextPresenter _scrollableText;
    private ITextPresenter _staticText;
    private ITextPresenter _currentText;

    public GameVideoControl()
    {
        _isDisabled = true;
    }

    public void Initialize(VideoPlayer videoPlayer)
    {
        if (_videoPlayer != null)
            Unsubscribe();

        _videoPlayer = videoPlayer;

        if (_canvasObject is null)
        {
            _canvasObject = CreateSubtitlesCanvas();
            _subtitlesText = CreateSubtitlesText(_canvasObject.transform);
            _scrollableText = TextScroller.Create(_canvasObject.transform);
            _staticText = CreateStaticText(_canvasObject.transform);
            _canvasObject.SetActive(false);
            _currentText = _subtitlesText;
        }

        Subscribe();
    }

    private void Subscribe()
    {
        _videoPlayer.started += OnVideoStarted;
        _videoPlayer.prepareCompleted += OnVideoPrepared;
        _videoPlayer.loopPointReached += OnVideoFinished;
        _videoPlayer.errorReceived += OnVideoError;
    }

    private void Unsubscribe()
    {
        _isDisabled = true;

        _canvasObject.SetActive(false);
        _currentText = null;
        _staticText = null;
        _scrollableText = null;
        _subtitlesText = null;
        GameObject.Destroy(_canvasObject);
        _canvasObject = null;

        _videoPlayer.started -= OnVideoStarted;
        _videoPlayer.prepareCompleted -= OnVideoPrepared;
        _videoPlayer.loopPointReached -= OnVideoFinished;
        _videoPlayer.errorReceived -= OnVideoError;
        _videoPlayer = null;
        _srtParser = null;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        ModComponent.Log.LogMessage($"[Video] Prepared: {source.url}");
        String srtPath = Path.GetDirectoryName(source.url) + '/' + Path.GetFileNameWithoutExtension(source.url) + $"_{Localization.GetLanguageSufix()}.srt";
        _srtParser = SRTParser.FromFile(srtPath);

        ModComponent.Log.LogMessage($"[Video] HaveSubtitles: {_srtParser.HaveSubtitles}");
        _canvasObject.SetActive(_srtParser.HaveSubtitles);
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        ModComponent.Log.LogMessage($"[Video] Started: {source.url}");
        _isDisabled = false;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        ModComponent.Log.LogMessage($"[Video] Finished: {source.url}");
        Unsubscribe();
    }

    private void OnVideoError(VideoPlayer source, String message)
    {
        ModComponent.Log.LogMessage($"[Video] Error: {source.url}, {message}");

        Unsubscribe();
    }

    protected override void Update()
    {
        SubtitleBlock subtitle = _srtParser.GetForTime(_videoPlayer.time);
        if (ReferenceEquals(subtitle, _current))
        {
            _currentText?.Update();
            return;
        }

        _current = subtitle;
        _videoPlayer.playbackSpeed = 1.0f;
        _currentText.SetText(String.Empty);
        _currentText.SetActive(false);
        if (ReferenceEquals(subtitle, null))
            return;

        _currentText = _subtitlesText;

        String text = subtitle.Text;
        if (text.StartsWith('#'))
        {
            using (StringReader sr = new StringReader(text))
            {
                while (sr.Peek() == '#')
                {
                    String controlSequence = sr.ReadLine();
                    String[] pair = controlSequence.Split('=');

                    HandleControlSequence(pair[0], pair[1], ref _currentText);
                }

                text = sr.ReadToEnd();
            }
        }

        _currentText.SetText(text);
        _currentText.SetActive(true);
        //
        //
        //
        // if (subtitle.Index == 1)
        // {
        //     if (!string.IsNullOrEmpty(text))
        //     {
        //         isEmpty = false;
        //         subtitlesText.SetText(text);
        //         return;
        //     }
        // }
        // else if (subtitle.Index == 2)
        // {
        //     if (!string.IsNullOrEmpty(text))
        //     {
        //         isEmpty = false;
        //
        //         scrollableText.SetText(text);
        //
        //         if (!scrollableTextPanel.activeSelf)
        //         {
        //             scrollableTextPanel.SetActive(true);
        //             scrollText = true;
        //         }
        //
        //         return;
        //     }
        // }
        // else if (subtitle.Index == 3)
        // {
        //     if (!string.IsNullOrEmpty(text))
        //     {
        //         isEmpty = false;
        //
        //         staticText.SetText(text);
        //
        //         if (!staticTextPanel.activeSelf)
        //         {
        //             staticTextPanel.SetActive(true);
        //         }
        //
        //         return;
        //     }
        // }
        //
        // ModComponent.Log.LogMessage("Text: " + text);
        // _subtitlesText.SetText(text);
    }

    private void HandleControlSequence(String key, String value, ref ITextPresenter textPresenter)
    {
        switch (key)
        {
            case "#playbackSpeed":
            {
                HandlePlaySpeed(value);
                break;
            }
            case "#subtitlePresenter":
            {
                HandlePresenter(value, out textPresenter);
                break;
            }
            default:
            {
                throw new NotSupportedException(key);
            }
        }
    }

    private void HandlePresenter(String value, out ITextPresenter textPresenter)
    {
        textPresenter = value switch
        {
            "subtitle" => _subtitlesText,
            "scrollable" => _scrollableText,
            "static" => _staticText,
            _ => throw new NotSupportedException(value)
        };
    }

    private void HandlePlaySpeed(String value)
    {
        Single speed = float.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        _videoPlayer.playbackSpeed = speed;
    }

    private static GameObject CreateSubtitlesCanvas()
    {
        Canvas canvas = UIBuilder.CreateCanvas("SUBTITLES CANVAS", parent: null);
        {
            CanvasScaler cs = canvas.gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.matchWidthOrHeight = 1;
            cs.referenceResolution = new Vector2(1920, 1080);
        }

        return canvas.gameObject;
    }

    private ITextPresenter CreateSubtitlesText(Transform parent)
    {
        TextMeshProUGUI subtitlesText = UIBuilder.CreateUiObject("SubtitlesText", parent).AddComponent<TextMeshProUGUI>();
        UIElementSetAnchors(subtitlesText.gameObject, 0.05f, 0, 0.95f, 0.2f);
        UIElementSetOffsets(subtitlesText.gameObject);

        subtitlesText.alignment = TextAlignmentOptions.Top;
        subtitlesText.enableAutoSizing = true;
        subtitlesText.fontSizeMin = 8;
        subtitlesText.fontSizeMax = 60;
        subtitlesText.outlineWidth = 0.85f;
        subtitlesText.outlineColor = Color.black;
        
        return new TextPresenter(subtitlesText, subtitlesText.gameObject);
    }

    private static ITextPresenter CreateStaticText(Transform parent)
    {
        Image staticTextPanel = CreatePanel("StaticTextPanel", parent, false, 0.99f);
        UIElementSetAnchors(staticTextPanel.gameObject, 0.105f, 0.27f, 0.9f, 0.87f);

        TextMeshProUGUI staticText = UIBuilder.CreateUiObject("StaticText", staticTextPanel.transform).AddComponent<TextMeshProUGUI>();
        {
            UIElementSetAnchors(staticText.gameObject, 0, 0, 1, 1);
            UIElementSetOffsets(staticText.gameObject, 30, 15, -30, -15);

            staticText.wordWrappingRatios = 0.5f;
            staticText.overflowMode = TextOverflowModes.Ellipsis;
            staticText.alignment = TextAlignmentOptions.Justified;

            staticText.enableAutoSizing = true;
            staticText.fontSizeMin = 8;
            staticText.fontSizeMax = 90;
            staticText.outlineWidth = 0;
            staticText.characterSpacing = 10;
            staticText.lineSpacing = -10;
            staticText.paragraphSpacing = 50;

            return new TextPresenter(staticText, staticTextPanel.gameObject);
        }
    }

    private static Image CreatePanel(String name, Transform parent, Boolean activeOnStart, Single alpha)
    {
        GameObject panel = UIBuilder.CreateUiObject(name, parent);
        {
            Image panelImage = panel.AddComponent<Image>();
            {
                Color panelColor = Color.black;
                panelColor.a = alpha;
                panelImage.color = panelColor;
            }

            UIElementSetAnchors(panel, 0, 0, 1, 1);
            UIElementSetOffsets(panel);

            panel.SetActive(activeOnStart);
            return panelImage;
        }
    }

    private static void UIElementSetAnchors(GameObject uiElement, Single MinX = 0, Single MinY = 1, Single MaxX = 0, Single MaxY = 1)
    {
        RectTransform _textRectTransform = uiElement.GetComponent<RectTransform>();
        _textRectTransform.anchorMin = new Vector2(MinX, MinY);
        _textRectTransform.anchorMax = new Vector2(MaxX, MaxY);
    }

    private static void UIElementSetOffsets(GameObject uiElement, Single left = 0, Single bottom = 0, Single right = 0, Single top = 0)
    {
        RectTransform _textRectTransform = uiElement.GetComponent<RectTransform>();
        _textRectTransform.offsetMin = new Vector2(left, bottom);
        _textRectTransform.offsetMax = new Vector2(right, top);
    }

    interface ITextPresenter
    {
        void Update();
        void SetText(String text);
        void SetActive(Boolean isActive);
    }
    
    private sealed class TextPresenter : ITextPresenter
    {
        private readonly TextMeshProUGUI _text;
        private readonly GameObject _activator;

        public TextPresenter(TextMeshProUGUI text, GameObject activator)
        {
            _text = text;
            _activator = activator;
        }
        
        public void Update()
        {
        }

        public void SetText(String text)
        {
            _text.SetText(text);
        }

        public void SetActive(Boolean isActive)
        {
            _activator.SetActive(isActive);
        }
    }

    private sealed class TextScroller : ITextPresenter
    {
        private readonly TextMeshProUGUI _scrollableText;
        private readonly Image _activator;
        private readonly RectTransform _scrollableTextTransform;

        private readonly Single _scrollSpeedMultiplier = 20;
        private readonly Int32 _scrollTime = 15;
        private readonly Vector2 _targetPosition = new(0, 1120);

        private Single _currentSpeed = 1;

        public TextScroller(TextMeshProUGUI scrollableText, Image activator)
        {
            _scrollableText = scrollableText;
            _activator = activator;
            _scrollableTextTransform = _scrollableText.rectTransform;
        }

        public static TextScroller Create(Transform parent)
        {
            Image scrollableTextPanel = CreatePanel("ScrollViewPanel", parent, false, 0.00f);
            UIElementSetAnchors(scrollableTextPanel.gameObject, 0.52f, 0, 1, 1);

            TextMeshProUGUI scrollableText = UIBuilder.CreateUiObject("ScrollableText", scrollableTextPanel.transform).AddComponent<TextMeshProUGUI>();

            scrollableText.transform.SetParent(scrollableTextPanel.transform);
            UIElementSetAnchors(scrollableText.gameObject, 0.05f, 0, 0.95f, 1);
            UIElementSetOffsets(scrollableText.gameObject, 0, -1220, 0, -1080);

            scrollableText.wordWrappingRatios = 0.5f;
            scrollableText.overflowMode = TextOverflowModes.Ellipsis;
            scrollableText.alignment = TextAlignmentOptions.TopJustified;

            scrollableText.enableAutoSizing = true;
            scrollableText.fontSizeMin = 8;
            scrollableText.fontSizeMax = 90;
            scrollableText.outlineWidth = 0;
            scrollableText.characterSpacing = 10;
            scrollableText.lineSpacing = -10;
            scrollableText.paragraphSpacing = 50;
            return new TextScroller(scrollableText, scrollableTextPanel);
        }
        
        public void Update()
        {
            ScrollText();
        }
        
        public void SetText(String text)
        {
            _scrollableText.SetText(text);
            if (String.IsNullOrEmpty(text))
                SetActive(false);
        }

        public void SetActive(Boolean isActive)
        {
            _activator.gameObject.SetActive(isActive);
        }
        
        private void ScrollText()
        {
            if (!_activator.gameObject.activeInHierarchy)
                return;

            _currentSpeed = _scrollSpeedMultiplier * 60 / _scrollTime * Time.deltaTime;
            _scrollableTextTransform.anchoredPosition = Vector2.MoveTowards(_scrollableTextTransform.anchoredPosition, _targetPosition, _currentSpeed);

            // if (_scrollableTextTransform.anchoredPosition.y >= _targetPosition.y - _currentSpeed * 1.5f)
            // {
            //     Color targetColor = Color.black;
            //     targetColor.a = 0.98f;
            //     _activator.CrossFadeColor(targetColor, duration: 1.0f, ignoreTimeScale: false, useAlpha: true);
            // }
            //
            // if (_scrollableTextTransform.anchoredPosition.y >= _targetPosition.y)
            // {
            //     _activator.gameObject.SetActive(false);
            // }
        }
    }
}