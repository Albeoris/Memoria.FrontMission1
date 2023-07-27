using System.Collections.Generic;
using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

internal sealed class SelectionProxy
{
    private static readonly AccessTools.FieldRef<Selection, List<MapCell>> m_VisibleCells = AccessTools.FieldRefAccess<Selection, List<MapCell>>(AccessTools.Field(typeof(Selection), "m_VisibleCells"));

    public Selection Selection { get; }

    public SelectionProxy(Selection selection)
    {
        Selection = selection;
    }

    public List<MapCell> VisibleCells => m_VisibleCells(Selection);
}