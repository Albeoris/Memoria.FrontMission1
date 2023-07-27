using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

[HarmonyPatch(typeof(HoldFunctions), "ShowMoveRange")]
public static class HoldFunctions_ShowMoveRange
{
    private static void Postfix(MapCell cell)
    {
        if (!ModComponent.Instance.Config.Battlefield.DisplayAttackRangeWithMovement)
            return;
        
        Wanzer wanzer = cell.GetWanzer();
        if (wanzer is null)
            return;
        
        HashSet<MapCell> cells = new();

        Selection selection = ScenarioScene.Instance.Selection;
        foreach (WalkerGridNode node in wanzer.AvailableCells)
            cells.Add(node.MapCell);
        
        IReadOnlyList<Weapon> availableWeapon = wanzer.Body.GetAllWeapons().Where(w => w.IsInfinityAmmunition || w.AttackCount >= 1).ToArray();
        
        foreach (MapCell visibleCell in cells.ToArray())
        {
            foreach (Weapon weapon in availableWeapon)
            {
                CellsCollection weaponCellsRange = MapCellSystem.Instance.GetWeaponCellsRange(visibleCell, weapon.Range_Min, weapon.Range_Max);

                for (Int32 index = 0; index < weaponCellsRange.cells.Length; ++index)
                {
                    if (!weaponCellsRange.cells[index])
                        continue;
                    
                    MapCell mapCell = MapCellSystem.Instance.GetCell(index);
                    if (cells.Add(mapCell))
                        selection.ShowCell(mapCell, wanzer.IsEnemy);
                }
            }   
        }
    }
}