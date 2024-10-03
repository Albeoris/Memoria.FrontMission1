using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Memoria.FrontMission1.Configuration;
using Memoria.FrontMission1.Core;
using Memoria.FrontMission1.HarmonyHooks;
using UnityEngine;
using Walker;

namespace Memoria.FrontMission1.Export;

public sealed class ResourceExporter : MonoBehaviour
{
    private String _exportDirectory;
    private Int32 _currentIndex;
    private Int32 _totalCount = 1;
    private Int32 _skippedCount;

    private Texture2D _blackTexture;
    private GUIStyle _guiStyle;
    private String _currentOperation = "Exporting";
    private Boolean _overwrite;

    public void Awake()
    {
        try
        {
            AssetsConfiguration config = ModComponent.Instance.Config.Assets;
            _exportDirectory = config.GetExportDirectoryIfEnabled();
            if (_exportDirectory == String.Empty)
            {
                ModComponent.Log.LogInfo($"[Export] Export skipped. Export directory is not defined.");
                Destroy(this);
                return;
            }
            
            _overwrite = config.ExportOverwrite;

            // OnGui
            _blackTexture = new Texture2D(1, 1);
            _blackTexture.SetPixel(0, 0, Color.black);
            _blackTexture.Apply();

            _guiStyle = new GUIStyle();
            _guiStyle.fontSize = 48;
            _guiStyle.normal.textColor = Color.white;
            _guiStyle.alignment = TextAnchor.MiddleCenter;

            StartCoroutine(Export());
        }
        catch (Exception ex)
        {
            OnExportError(ex);
        }
    }

    private IEnumerator<YieldInstruction> Export()
    {
        try
        {
            AssetsConfiguration config = ModComponent.Instance.Config.Assets;

            if (config.ExportLocalization)
                yield return StartCoroutine(ExportLocalization());

            ModComponent.Log.LogInfo($"[Export ({_currentIndex} / {_totalCount})] Assets exported successfully.");
            if (_skippedCount > 0)
            {
                ModComponent.Log.LogInfo($"[Export ({_currentIndex} / {_totalCount})] {_skippedCount} assets are skipped as they have already been exported before.");
                if (!ModComponent.Instance.Config.Assets.ExportLogAlreadyExportedAssets)
                    ModComponent.Log.LogInfo($"For detailed information, set the option Assets.ExportLogAlreadyExportedAssets = true");
            }

            if (ModComponent.Instance.Config.Assets.ExportAutoDisable)
                ModComponent.Instance.Config.Assets.ExportEnabled = false;
        }
        finally
        {
            Destroy(this);
        }
    }

    private IEnumerator<YieldInstruction> ExportLocalization()
    {
        _currentOperation = "Waiting for localization resources...";
        _currentIndex = 0;
        _totalCount = 0;

        ModComponent.Log.LogInfo($"[ExportLocalization] {_currentOperation}.");
        while (!Localization.IsReady)
            yield return new WaitForSeconds(1);
        
        Time.timeScale = 0.0f;
        ModComponent.Log.LogInfo($"[Export] Game stopped. Export started. Directory: {_exportDirectory}");

        Localization.GameLanguage[] gameLanguages = (Localization.GameLanguage[])Enum.GetValues(typeof(Localization.GameLanguage));
        _totalCount = Localization.current.GetCount() * (gameLanguages.Length - 1) * 2;

        foreach (Localization.GameLanguage language in gameLanguages)
        {
            if (language == Localization.GameLanguage.Default)
                continue;

            _currentOperation = $"Exporting {language} localization...";
            ModComponent.Log.LogInfo($"[ExportLocalization] {_currentOperation}");
            Localization.ChangeLanguage(language);

            String outputDirectory = Path.Combine(_exportDirectory, "Localization", language.ToString());
            Directory.CreateDirectory(outputDirectory);

            String[] tags = SyncronizeTags(outputDirectory, language);

            List<Line> lines = new List<Line>();
            LocalizedMessages localizedMessages = Localization.current;
            Int32 count = localizedMessages.GetCount();
            for (Int32 i = 0; i < count; i++)
            {
                localizedMessages.GetAt(i, out String key, out LocalizedMessages.MessageInfo text);
                lines.Add(new Line(key, text.m_Value));
            }

            var regexes = tags.OrderByDescending(t => t.Length).Select(t => new KeyValuePair<String, String>(t, t[0] + "�" + t.Substring(1))).ToArray();
            foreach (Line line in lines)
            {
                Prepare(line);
                _currentIndex++;
                if (_currentIndex % 1000 == 0)
                    yield return null; // to refresh progress
            }

            void Prepare(Line line)
            {
                var prepareMessage = line.Value;

                foreach (var regex in regexes)
                {
                    var m = prepareMessage.Replace(regex.Key, '{' + regex.Value + '}');
                    if (m != prepareMessage)
                        prepareMessage = m;
                }

                prepareMessage = prepareMessage.Replace("�", "");
                line.Value = prepareMessage;
            }
            
            IEnumerable<IGrouping<String, Line>> groups = lines.GroupBy(ResolveGroupName);
            
            List<Line> other = new List<Line>();
            foreach (IGrouping<string, Line> group in groups)
            {
                Line[] items = group.ToArray();
                if (items.Length < 3 || group.Key == "other")
                {
                    other.AddRange(items);
                    continue;
                }
                
                String outPath = Path.Combine(outputDirectory, group.Key + ".json");
                OrderedDictionary<String, TransifexEntry> map = new OrderedDictionary<String, TransifexEntry>();
                Int32 mapSize = 0;
                Boolean hasNotEmptyLines = false;
                foreach (Line line in group)
                {
                    hasNotEmptyLines |= !String.IsNullOrEmpty(line.Value);
                    map.TryAdd(line.Key, new TransifexEntry() { Text = line.Value, Context = group.Key});
                    mapSize++;
                }
                if (hasNotEmptyLines)
                    StructuredJson.Write(outPath, map);
                _currentIndex += mapSize;
                yield return null;
            }

            String outPath2 = Path.Combine(outputDirectory, "other.json");
            {
                OrderedDictionary<String, TransifexEntry> map = new();
                Int32 mapSize = 0;
                Boolean hasNotEmptyLines = false;
                foreach (Line line in other)
                {
                    hasNotEmptyLines |= !String.IsNullOrEmpty(line.Value);
                    map.TryAdd(line.Key, new TransifexEntry() { Text = line.Value, Context = ResolveGroupName(line)});
                    mapSize++;
                }
                if (hasNotEmptyLines)
                    StructuredJson.Write(outPath2, map);
                _currentIndex += mapSize;
                yield return null;
            }
        }
    }

    private static String ResolveGroupName(Line l)
    {
        Int32 index = l.Key.LastIndexOf('_');
        if (index < 0)
            return "other";
                    
        return l.Key.Substring(0, l.Key.LastIndexOf('_'));
    }

    private String[] SyncronizeTags(String outputDirectory, Localization.GameLanguage language)
    {
        if (language != Localization.GameLanguage.English)
            return []; // not supported

        String outputFile = Path.Combine(outputDirectory, "Tags.json");
        if (HandleExistingFile(outputFile))
            return EnglishTags; // todo read external tags

        List<Line> tags = new List<Line>();
        foreach (String tag in EnglishTags)
            tags.Add(new Line($"{tag}", tag));
        
        OrderedDictionary<String, TransifexEntry> map = new();
        foreach (Line line in tags)
        {
            map.TryAdd(line.Key, new TransifexEntry() { Text = line.Value, Context = "Tags"});
        }
        StructuredJson.Write(outputFile, map);

        return EnglishTags;
    }

    private static readonly String[] EnglishTags = new[]
    {
        "Agent",
        "Alder",
        "Allen",
        "Alpha",
        "Announcer",
        "Arena",
        "Arena Manager",
        "Arms Dealer",
        "B.A. Mine",
        "Balda",
        "Barria",
        "Bart",
        "Bartender",
        "Beretta",
        "Bill",
        "Billy",
        "Black Hounds",
        "Blakewood",
        "Bob",
        "Bobby",
        "CPU",
        "Cain",
        "Captain",
        "Chris",
        "City",
        "Colonel",
        "Crows",
        "Dan",
        "Darril",
        "Dave",
        "Dealer",
        "Dr. Brown",
        "Driscoll",
        "Efil",
        "Fisherman",
        "Fort",
        "Frederick",
        "Gail",
        "Gain",
        "Garuna",
        "Gene",
        "General",
        "Gentz",
        "Ghetta",
        "Gilmore",
        "Gina",
        "Glen",
        "Grey",
        "Grieg",
        "Griff",
        "Guard",
        "Guerrilla",
        "Halle",
        "Hans",
        "Hash",
        "Hector",
        "Helen",
        "Herbert",
        "Hounds",
        "Howard",
        "Huffman",
        "Irina",
        "J. J.",
        "Jean",
        "Joanna",
        "Johnny",
        "Josh",
        "Jubert",
        "Karen",
        "Keith",
        "Kelly",
        "Kill Bonus",
        "Kira",
        "Kirkland",
        "Koichi",
        "Koichi Sakata",
        "Kong",
        "Laurent",
        "Libal",
        "Lieutenant",
        "Lizzy",
        "Man in Black",
        "Man in Shades",
        "Maria",
        "Marine",
        "Matthew",
        "Mayor",
        "Mei",
        "Meihua",
        "Mercenary",
        "Meryl",
        "Michael",
        "Miller",
        "Milligan",
        "Minanta",
        "Mission",
        "Molly",
        "Monus",
        "Moon",
        "Mussar",
        "Natalie",
        "Niles",
        "Nirvana",
        "Nirvana Agent",
        "Noster",
        "Officer",
        "Olson",
        "Patricia",
        "Paul",
        "Peewie",
        "Peseta Comms",
        "Porunga",
        "Ralph",
        "Randy",
        "Rebus",
        "Reiji",
        "Renges",
        "Royd",
        "Ryuji",
        "Saban",
        "Sakata",
        "Scientist",
        "Scout",
        "Selleh",
        "Sir",
        "Skyeye",
        "Soldier",
        "Spec Ops Agent",
        "Steve",
        "Stewart",
        "Steya",
        "Toiko",
        "Villager",
        "Walter",
        "Wanzer",
        "Winger",
        "Woman in Suit",
        "Yeehin",
        "Zabia",
        "Zett",
        "wanzer",
        "U.C.S.",
        "O.C.U."
    };

    private sealed class Line
    {
        public String Key { get; }
        public String Value { get; set; }

        public Line(String key, String value)
        {
            Key = key;
            Value = value;
        }
    }

    public void OnGUI()
    {
        GUI.skin.box.normal.background = _blackTexture;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

        if (_totalCount == 0)
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"{_currentOperation}", _guiStyle);
        }
        else
        {
            Single progress = (100.0f * _currentIndex) / _totalCount;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"{_currentOperation} ({progress:F2}%): {_currentIndex} / {_totalCount}", _guiStyle);
        }
    }

    public void OnDisable()
    {
        try
        {
            ModComponent.Log.LogInfo($"[Export] Export stopped.");
            if (_exportDirectory != String.Empty)
                Application.Quit();
        }
        catch (Exception ex)
        {
            OnExportError(ex);
        }
    }

    private Boolean HandleExistingDirectory(String assetName, String fullPath, Boolean overwrite)
    {
        if (!Directory.Exists(fullPath))
            return false;

        if (overwrite)
        {
            if (!fullPath.Contains("FRONT MISSION"))
            {
                String shortPath = ApplicationPathConverter.ReturnPlaceholders(fullPath);
                ModComponent.Log.LogWarning($"[Export ({_currentIndex} / {_totalCount})] \tDirectory {fullPath} cannot be deleted. Delete it manually if you think it's safe.");
                ModComponent.Log.LogWarning($"[Export ({_currentIndex} / {_totalCount})] \tSkip exporting an existing directory of asset [{assetName}]: {shortPath}");
                _skippedCount++;
                return true;
            }

            Directory.Delete(fullPath, recursive: true);
        }
        else
        {
            if (ModComponent.Instance.Config.Assets.ExportLogAlreadyExportedAssets)
            {
                String shortPath = ApplicationPathConverter.ReturnPlaceholders(fullPath);
                ModComponent.Log.LogInfo($"[Export ({_currentIndex} / {_totalCount})] \tSkip exporting an existing directory of asset [{assetName}]: {shortPath}");
            }

            _skippedCount++;

            return true;
        }

        return false;
    }

    private Boolean HandleExistingFile(String fullPath)
    {
        if (HandleExistingFile($"Export ({_currentIndex} / {_totalCount})", fullPath))
        {
            _skippedCount++;
            return true;
        }

        return false;
    }

    public Boolean HandleExistingFile(String logPrefix, String fullPath)
    {
        if (!File.Exists(fullPath))
            return false;

        if (_overwrite)
        {
            if (!fullPath.Contains("FRONT MISSION"))
            {
                String shortPath = ApplicationPathConverter.ReturnPlaceholders(fullPath);
                ModComponent.Log.LogWarning($"[{logPrefix}] \tFile {fullPath} cannot be deleted. Delete it manually if you think it's safe.");
                ModComponent.Log.LogWarning($"[{logPrefix}] \tSkip exporting an existing directory of asset: {shortPath}");
                return true;
            }

            File.Delete(fullPath);
            return false;
        }

        if (ModComponent.Instance.Config.Assets.ExportLogAlreadyExportedAssets)
        {
            String shortPath = ApplicationPathConverter.ReturnPlaceholders(fullPath);
            ModComponent.Log.LogInfo($"[{logPrefix}] \tSkip exporting an existing file of asset: {shortPath}");
        }

        return true;
    }

    private void OnExportError(Exception exception)
    {
        ModComponent.Log.LogError($"[Export] Failed to export assets: {exception}");
        Destroy(this);
    }
}