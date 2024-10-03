using System;
using System.Collections.Generic;

namespace Memoria.FrontMission1.Mods;

public interface IModFileResolver
{
    void TryUpdate();
    IReadOnlyList<String> FindAllStartedWith(String assetsRoot);
    IReadOnlyList<String> FindAll(String partialPath);
}