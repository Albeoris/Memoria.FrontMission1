using System;
using System.Globalization;
using System.IO;
using FE;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Walker;
using Path = System.IO.Path;

namespace Memoria.FrontMission1.Core;

public sealed class GameVideoControl : SafeComponent
{
    private VideoPlayer _videoPlayer;
    private SRTParser _srtParser;
    private SubtitleBlock _current;

    private GameObject _canvasObject;
    private TextMeshProUGUI _subtitlesText;

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
            _subtitlesText = CreateSubtitlesText();
            _canvasObject.SetActive(false);
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
        _canvasObject.SetActive(false);
        _subtitlesText.SetText(String.Empty);
        _isDisabled = true;
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
            return;

        _current = subtitle;
        _videoPlayer.playbackSpeed = 1.0f;
        if (ReferenceEquals(subtitle, null))
        {
            _subtitlesText.SetText(String.Empty);
            return;
        }

        String text = subtitle.Text;
        if (text.StartsWith('#'))
        {
            using (StringReader sr = new StringReader(text))
            {
                while (sr.Peek() == '#')
                {
                    String controlSequence = sr.ReadLine();
                    String[] pair = controlSequence.Split('=');

                    HandleControlSequence(pair[0], pair[1]);
                }

                text = sr.ReadToEnd();
            }
        }

        ModComponent.Log.LogMessage("Text: " + text);
        _subtitlesText.SetText(text);
    }

    private void HandleControlSequence(String key, String value)
    {
        switch (key)
        {
            case "#playbackSpeed":
            {
                HandlePlaySpeed(value);
                break;
            }
            default:
            {
                throw new NotSupportedException(key);
            }
        }
    }

    private void HandlePlaySpeed(String value)
    {
        Single speed = float.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        _videoPlayer.playbackSpeed = speed;
    }

    private static GameObject CreateSubtitlesCanvas()
    {
        GameObject canvasObject = new GameObject($"{ModConstants.Id}: SUBTITLES CANVAS");
        {
            canvasObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.layer = LayerMask.NameToLayer("UI");
        }

        return canvasObject;
    }

    private TextMeshProUGUI CreateSubtitlesText()
    {
        Int32 subtitlesLabelWidth = Screen.width - 100;
        Int32 subtitlesLabelHeight = 160;

        TextMeshProUGUI subtitlesText = new GameObject($"{ModConstants.Id}: IntroSubtitles").AddComponent<TextMeshProUGUI>();
        {
            subtitlesText.gameObject.layer = LayerMask.NameToLayer("UI");
            subtitlesText.transform.SetParent(_canvasObject.transform);
            subtitlesText.rectTransform.sizeDelta = new Vector2(subtitlesLabelWidth, subtitlesLabelHeight);
            subtitlesText.transform.localPosition = new Vector3(0, -Screen.height / 2 + subtitlesLabelHeight / 2 + 20, 0);
            subtitlesText.alignment = TextAlignmentOptions.Top;
            subtitlesText.fontSize = 40;
            subtitlesText.outlineWidth = 0.85f;
            subtitlesText.outlineColor = Color.black;
        }

        return subtitlesText;
    }
}