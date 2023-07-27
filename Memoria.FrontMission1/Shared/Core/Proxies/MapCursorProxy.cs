using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

internal sealed class MapCursorProxy
{
    private static readonly AccessTools.FieldRef<MapCursor, Wanzer> _previousWanzer = AccessTools.FieldRefAccess<MapCursor, Wanzer>(AccessTools.Field(typeof(MapCursor), "previousWanzer"));

    public static MapCursorProxy Instance => new(MapCursor.Instance);

    public MapCursor MapCursor { get; }

    public MapCursorProxy(MapCursor mapCursor)
    {
        MapCursor = mapCursor;
    }

    public Wanzer PreviousWanzer
    {
        get => _previousWanzer(MapCursor);
        set => _previousWanzer(MapCursor) = value;
    }
}