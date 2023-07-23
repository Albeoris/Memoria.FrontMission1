using System;
using Walker;

namespace Memoria.FrontMission1.HarmonyHooks;

public static class LocalizationHelper
{
    public static Boolean TryGet(String key, out String value)
    {
        LocalizedMessages localizedMessages = Localization.current;
        if (localizedMessages is null)
        {
            value = null;
            return false;
        }
        
        String result = localizedMessages.Get(key);
        if (result.StartsWith('@'))
        {
            value = default;
            return false;
        }

        value = result;
        return true;
    }
}