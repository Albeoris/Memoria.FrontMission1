using System;
using HarmonyLib;
using Memoria.FrontMission1.BeepInEx;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(MapCursor), "CheckHoldFunctions")]
public static class MapCursor_CheckHoldFunctions
{
    private static MapCell _previousCell;
    
    private static void Prefix(MapCursor __instance, ref MapCell cell, PhaseManager ___m_PhaseManager, HoldFunctions ___m_holdFunctions, ref Wanzer ___previousWanzer, out State __state)
    {
        __state = default;

        try
        {
            if (!ModComponent.Instance.Config.Battlefield.DisplayAttackRangeOnCell)
                return;
            
            if (!___m_holdFunctions.isShowWeaponRangeActive)
                return;
        
            Phase currentPhase = ___m_PhaseManager.CurrentPhase;
            if (currentPhase != ___m_PhaseManager.PlayerPhase)
                return;

            Wanzer currentWanzer = currentPhase.CurrentEntity;
            if (currentWanzer is null)
                return;

            Wanzer wanzerUnderCursor = cell.GetWanzer();
            if (wanzerUnderCursor is not null)
                return;

            if (_previousCell != cell)
            {
                ___previousWanzer = null;
                _previousCell = cell;
            }
            
            __state = new State(currentWanzer, currentWanzer.MapCell, cell);
            __state.ChangeCell();
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }

    private static void Postfix(MapCell cell, State __state)
    {
        try
        {
            if (__state is null)
                return;

            __state.ChangeBack();
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
    
    private sealed class State
    {
        private readonly WanzerProxy _wanzer;
        private readonly MapCell _originalCell;
        private readonly MapCell _targetCell;

        public State(Wanzer wanzer, MapCell originalCell, MapCell targetCell)
        {
            _wanzer = new WanzerProxy(wanzer);
            _originalCell = originalCell;
            _targetCell = targetCell;
        }

        public void ChangeCell()
        {
            _wanzer.MapCell = _targetCell;
            _targetCell.SetWanzer(_wanzer.Wanzer);
        }

        public void ChangeBack()
        {
            _wanzer.MapCell = _originalCell;
            _targetCell.SetWanzer(null);
        }
    }
}