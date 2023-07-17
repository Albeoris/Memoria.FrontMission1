using System;
using BepInEx.Logging;

namespace Memoria.FrontMission1.Configuration;

public sealed partial class ModConfiguration
{
    public SpeedConfiguration Speed { get; }
    public AssetsConfiguration Assets { get; }

    public ModConfiguration()
    {
        using (var log = Logger.CreateLogSource("Memoria Config"))
        {
            try
            {
                log.LogInfo($"Initializing {nameof(ModConfiguration)}");

                ConfigFileProvider provider = new();
                Speed = SpeedConfiguration.Create(provider);
                Assets = AssetsConfiguration.Create(provider);

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