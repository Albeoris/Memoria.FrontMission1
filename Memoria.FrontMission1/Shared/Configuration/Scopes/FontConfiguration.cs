using System;

namespace Memoria.FrontMission1.Configuration;

[ConfigScope("Font")]
public abstract partial class FontConfiguration
{
    [ConfigEntry($"Allows you to set the minimum font size. Warning: some interface elements will not display correctly.")]
    public virtual Int32 MinFontSize { get; set; } = 0;

    public abstract void CopyFrom(FontConfiguration configuration);
}