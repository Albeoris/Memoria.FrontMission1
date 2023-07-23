using System;
using Walker;
using Walker.Data;

namespace Memoria.FrontMission1.Core;

public class ArenaWinsControl
{
    public void RememberWin(Int32 cityIndex, Walker.Data.Wanzer player, Walker.Data.Wanzer enemy)
    {
        String uniqueKey = GetUniqueKey(cityIndex, player.m_Pilot, enemy.m_MechIdx);
        Session.Instance.Prefs.SetBool(uniqueKey, true);
    }

    public Boolean HasAlreadyWin(Int32 cityIndex, Walker.Data.Wanzer player, Walker.Data.Wanzer enemy)
    {
        String uniqueKey = GetUniqueKey(cityIndex, player.m_Pilot, enemy.m_MechIdx);
        return Session.Instance.Prefs.GetBool(uniqueKey, false);
    }

    private String GetUniqueKey(Int32 cityIndex, PilotData playerPilot, Int32 enemyMechIdx)
    {
        return $"Mods.Memoria.ArenaWins.V1.{cityIndex}.{playerPilot}.{enemyMechIdx}";
    }
}