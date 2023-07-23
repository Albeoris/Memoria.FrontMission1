using System;
using Memoria.FrontMission1.Configuration.Hotkey;
using UnityEngine;

namespace Memoria.FrontMission1.Configuration;

[ConfigScope("Speed")]
public abstract partial class SpeedConfiguration
{
    private const String Section = "Speed";

    [ConfigEntry($"Speed up key." +
                 "$[Russian]: Кнопка для включения ускорения.")]
    [ConfigConverter(nameof(KeyConverter))]
    public virtual HotkeyGroup Key { get; } = HotkeyGroup.Create(new[] { new Hotkey.Hotkey(KeyCode.F1), new Hotkey.Hotkey(KeyCode.F1) { MustHeld = true } });

    [ConfigEntry($"Speed up toggle factor." +
                 "$[Russian]: Коэффициент ускорения при включении.")]
    public virtual Single ToggleFactor { get; } = 3.0f;

    [ConfigEntry($"Speed up hold factor." +
                 "$[Russian]: Коэффициент ускорения при удержании.")]
    public virtual Single HoldFactor { get; } = 5.0f;

    public abstract void CopyFrom(SpeedConfiguration configuration);

    protected IAcceptableValue<HotkeyGroup> KeyConverter { get; } = new AcceptableHotkeyGroup(nameof(Key), canHold: true);
}