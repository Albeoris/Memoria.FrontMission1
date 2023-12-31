﻿using System;
using BepInEx.Logging;

namespace Memoria.FrontMission1.Configuration;

public sealed partial class ModConfiguration
{
    public SpeedConfiguration Speed { get; }
    public SavesConfiguration Saves { get; }
    public AssetsConfiguration Assets { get; }
    public ArenaConfiguration Arena { get; }
    public BattlefieldConfiguration Battlefield { get; }
    public DebugConfiguration Debug { get; }

    public ModConfiguration()
    {
        using (var log = Logger.CreateLogSource("Memoria Config"))
        {
            try
            {
                log.LogInfo($"Initializing {nameof(ModConfiguration)}");

                ConfigFileProvider provider = new();
                Speed = SpeedConfiguration.Create(provider);
                Saves = SavesConfiguration.Create(provider);
                Assets = AssetsConfiguration.Create(provider);
                Arena = ArenaConfiguration.Create(provider);
                Battlefield = BattlefieldConfiguration.Create(provider);
                Debug = DebugConfiguration.Create(provider);

                log.LogInfo($"{nameof(ModConfiguration)} initialized successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to initialize {nameof(ModConfiguration)}: {ex}");
                throw;
            }
        }
    }
}