using System;
using FE;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(SaveSystem), "Save")]
public static class SaveSystem_Save
{
    public static void Postfix(Int32 slotId, SessionPrefs.SaveLocation location)
    {
        try
        {
            if (!ModComponent.Instance.Config.Saves.BackupSaveFiles)
                return;
            
            SaveSystem.IsSaving = true;
            SaveSystem.SlotID = slotId;
            Byte[] data = Session.Instance.OnSave(location, slotId);
            String name = $"{DateTime.Now:yyyy-MM-dd HH-mm-ss} - {location} - {Session.Instance.Prefs.GetString(80)}";
            StorageAsyncOp result = PublisherWrapper.Instance.CurrentPlayer.storage.Write(name, data, SaveSystem.OnSavingCompelte);
            if (result.success)
            {
                ModComponent.Log.LogMessage($"[Save] New file created: {name}");
            }
            else
            {
                ModComponent.Log.LogMessage($"[Save] Failed to create a save file: {result.status}. Path: {name}");
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}