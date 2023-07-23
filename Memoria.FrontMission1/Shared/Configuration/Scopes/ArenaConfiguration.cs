using System;

namespace Memoria.FrontMission1.Configuration;

[ConfigScope("Arena")]
public abstract partial class ArenaConfiguration
{
    [ConfigEntry("If enabled, each pilot will be able to fight only one time with each opponent in the arena." +
                 "$[Russian]: При включении, каждый пилот сможет сразиться лишь один раз с каждым противником на арене.")]
    public virtual Boolean DisableRepeatedBattles => false;

    public abstract void CopyFrom(ArenaConfiguration configuration);
}