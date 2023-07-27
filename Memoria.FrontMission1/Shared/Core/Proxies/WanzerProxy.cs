using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

internal sealed class WanzerProxy
{
    private static readonly AccessTools.FieldRef<Wanzer, MapCell> m_MapCell = AccessTools.FieldRefAccess<Wanzer, MapCell>(AccessTools.Field(typeof(Wanzer), "m_MapCell"));

    public Wanzer Wanzer { get; }

    public WanzerProxy(Wanzer wanzer)
    {
        Wanzer = wanzer;
    }

    public MapCell MapCell
    {
        get => Wanzer.MapCell;
        set => m_MapCell(Wanzer) = value;
    }
}