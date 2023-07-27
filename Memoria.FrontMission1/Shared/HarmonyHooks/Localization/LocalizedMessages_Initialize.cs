using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FE;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Memoria.FrontMission1.Configuration;
using Memoria.FrontMission1.Core;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(LocalizedMessages), "Initialize")]
public static class LocalizedMessages_Initialize
{
	public static void Postfix(LocalizedMessages __instance, Dictionary<String, LocalizedMessages.MessageInfo> ___m_LocalisationDict)
	{
		try
		{
			AssetsConfiguration config = ModComponent.Instance.Config.Assets;
			if (!config.ModsEnabled)
				return;

			String assetsRoot = $"Localization/{__instance.Language}";
			IReadOnlyList<String> files = ModComponent.Instance.ModFiles.FindAllStartedWith(assetsRoot);
			if (files.Count == 0)
				return;

			Modify(files, ___m_LocalisationDict);
		}
		catch (Exception ex)
		{
			ModComponent.Log.LogException(ex);
		}
	}

	private static void Modify(IReadOnlyList<String> files, Dictionary<String, LocalizedMessages.MessageInfo> messageInfos)
	{
		Dictionary<String, TransifexEntry> entries = new Dictionary<String, TransifexEntry>();
		Dictionary<String, TransifexEntry> tags = new Dictionary<String, TransifexEntry>();

		foreach (IGrouping<String, String> filesInFolder in files.GroupBy(Path.GetDirectoryName))
		{
			String[] ordered = filesInFolder.OrderBy(File.GetLastWriteTimeUtc).ToArray();
			ReadJson(ordered, entries, tags);
		}
		
		ApplyEntries(entries, messageInfos);
		ApplyTags(tags, messageInfos);
	}

	private static void ReadJson(IReadOnlyList<String> files, Dictionary<String, TransifexEntry> entries, Dictionary<String, TransifexEntry> tags)
	{
		foreach (String filePath in files)
		{
			try
			{
				String extension = Path.GetExtension(filePath);
				String fileName = Path.GetFileNameWithoutExtension(filePath);
				
				switch (extension)
				{
					case ".json":
					{
						ReadJson(filePath, fileName.Contains("Tags", StringComparison.InvariantCultureIgnoreCase) ? tags : entries);
						break;
					}
					default:
					{
						ModComponent.Log.LogWarning($"File with the {extension} extension cannot be used as a localization source. File: {filePath}");
						break;
					}
				}
			}
			catch (Exception ex)
			{
				ModComponent.Log.LogException(ex, $"Failed to apply file [{filePath}].");
			}
		}
	}

	private static void ReadJson(String filePath, Dictionary<String, TransifexEntry> entries)
	{
		OrderedDictionary<String, TransifexEntry> data = StructuredJson.Read(filePath);
		foreach ((String key, TransifexEntry value) in data)
		{
			String nativeKey = key.StartsWith("$") ? key.Substring(1) : key; // $msg_mena_bar1 -> msg_mena_bar1
			entries[nativeKey] = value;
		}
	}


	private static void ApplyEntries(Dictionary<String, TransifexEntry> entries, Dictionary<String, LocalizedMessages.MessageInfo> messageInfos)
	{
		Int32 changed = 0;
		Int32 added = 0;
		foreach ((String key, TransifexEntry value) in entries)
		{
			if (messageInfos.TryGetValue(key, out LocalizedMessages.MessageInfo info))
			{
				if (info.m_Value != value.Text)
				{
					info.m_Value = value.Text;
					changed++;
				}
			}
			else
			{
				info = new LocalizedMessages.MessageInfo { m_Value = value.Text };
				messageInfos.Add(key, info);
				added++;
			}

			if (Localization.HasNonEUChars(value.Text))
				info.m_Flags |= LocalizedMessages.MessageFlags.NonEULetters;
		}
		
		ModComponent.Log.LogMessage($"[{nameof(LocalizedMessages_Initialize)}] Applied: {entries.Count} localized files. Changed: {changed}, Added: {added}");
	}

	private static void ApplyTags(Dictionary<String, TransifexEntry> tags, Dictionary<String, LocalizedMessages.MessageInfo> messageInfos)
	{
		if (tags.Count == 0)
			return;

		Reference<TextReplacement>[] replacements = tags
			.Select(t => new Reference<TextReplacement>('{' + t.Key + '}', t.Value.Text))
			.ToArray();

		Int32 changed = 0;
		foreach ((String key, LocalizedMessages.MessageInfo value) in messageInfos)
		{
			String original = value.m_Value;
			value.m_Value = original.ReplaceAll(replacements);
			if (original != value.m_Value)
				changed++;
		}
		
		ModComponent.Log.LogMessage($"[{nameof(LocalizedMessages_Initialize)}] Applied: Tags.json. Changed: {changed}");
	}
}