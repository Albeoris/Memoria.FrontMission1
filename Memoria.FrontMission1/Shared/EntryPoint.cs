﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Memoria.FrontMission1.Core;
using Memoria.FrontMission1.HarmonyHooks;

namespace Memoria.FrontMission1;

[BepInPlugin(ModConstants.Id, "Memoria FRONT MISSION 1st: Remake", "1.0.0.0")]
public class EntryPoint : BaseUnityPlugin
{
    void Awake()
    {
        try
        {
            Logger.LogMessage("Initializing...");

            PatchCriticalMethods();
            
            SingletonInitializer singletonInitializer = new(Logger);
            singletonInitializer.InitializeInGameSingleton();
            
            PatchMethods();
            Logger.LogMessage("The mod has been successfully initialized.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize the mod: {ex}");
            throw;
        }
    }
    
    private void PatchCriticalMethods()
    {
        try
        {
            Logger.LogInfo("[Harmony] Patching critical methods...");
            Harmony harmony = new Harmony(ModConstants.Id);
            ConfigDescription_Description.Patch(harmony, Logger);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to patch methods.", ex);
        }
    }

    private void PatchMethods()
    {
        try
        {
            Logger.LogInfo("[Harmony] Patching methods...");
            Harmony harmony = new Harmony(ModConstants.Id);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in AccessTools.GetTypesFromAssembly(assembly))
            {
                PatchClassProcessor processor = harmony.CreateClassProcessor(type);
                if (processor.Patch()?.Count > 0)
                    Logger.LogInfo($"[Harmony] {type.Name} successfully applied.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to patch methods.", ex);
        }
    }
}