using System;
using System.Collections.Generic;

namespace Memoria.FrontMission1.Mods;

public sealed class EmptyModFileResolver : IModFileResolver
{
    public static IModFileResolver Instance { get; } = new EmptyModFileResolver();

    public void TryUpdate()
    {
    }

    public IReadOnlyList<String> FindAllStartedWith(String assetsRoot)
    {
        return Array.Empty<String>();
    }

    public IReadOnlyList<String> FindAll(String partialPath)
    {
        return Array.Empty<String>();
    }
}