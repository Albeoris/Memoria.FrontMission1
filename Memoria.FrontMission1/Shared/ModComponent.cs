﻿using System;
using BepInEx.Logging;
using Memoria.FrontMission1.Configuration;
using Memoria.FrontMission1.Core;
using Memoria.FrontMission1.Export;
using Memoria.FrontMission1.Mods;
using UnityEngine;
using Exception = System.Exception;
using Logger = BepInEx.Logging.Logger;

namespace Memoria.FrontMission1;

public sealed class ModComponent : MonoBehaviour
{
    public static ModComponent Instance { get; private set; }
    public static ManualLogSource Log { get; private set; }

    [field: NonSerialized] public ModConfiguration Config { get; private set; }
    [field: NonSerialized] public GameSpeedControl SpeedControl { get; private set; }
    [field: NonSerialized] public IModFileResolver ModFiles { get; private set; }
    [field: NonSerialized] public LocalizationControl Localization { get; private set; }
    [field: NonSerialized] public ArenaWinsControl ArenaWins { get; private set; }
    [field: NonSerialized] public GameVideoControl VideoControl { get; private set; }

    private Boolean _isDisabled;

    public void Awake()
    {
        Log = Logger.CreateLogSource("Memoria");
        Log.LogMessage($"[{nameof(ModComponent)}].{nameof(Awake)}(): Begin...");
        try
        {
            Instance = this;

            Config = new ModConfiguration();
            SpeedControl = new GameSpeedControl();
            ModFiles = Config.Assets.ExportEnabled ? EmptyModFileResolver.Instance : new ModFileResolver();
            Localization = new LocalizationControl();
            VideoControl = new GameVideoControl();

            ArenaWins = new ArenaWinsControl();
            
            gameObject.AddComponent<ResourceExporter>();

            Log.LogMessage($"[{nameof(ModComponent)}].{nameof(Awake)}(): Processed successfully.");
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {ex}");
            throw;
        }
    }
    
    public void OnDestroy()
    {
        Log.LogInfo($"[{nameof(ModComponent)}].{nameof(OnDestroy)}()");
    }

    private void FixedUpdate()
    {
        try
        {
            if (_isDisabled)
                return;
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(FixedUpdate)}(): {ex}");
        }
    }

    private void Update()
    {
        try
        {
            if (_isDisabled)
                return;

            ModFiles.TryUpdate();
            Localization.TryUpdate();
            VideoControl.TryUpdate();
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(Update)}(): {ex}");
        }
    }

    private void LateUpdate()
    {
        try
        {
            if (_isDisabled)
                return;

            SpeedControl.TryUpdate();
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {ex}");
        }
    }

    private void OnGUI()
    {
        try
        {
            if (_isDisabled)
                return;
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(OnGUI)}(): {ex}");
        }
    }
}