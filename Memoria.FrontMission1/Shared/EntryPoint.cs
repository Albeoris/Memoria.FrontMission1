using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Memoria.FrontMission1.Core;

namespace Memoria.FrontMission1.Shared;

[BepInPlugin(ModConstants.Id, "Memoria Songs Of Conquest", "1.0.0.0")]
public class EntryPoint : BaseUnityPlugin
{
    void Awake()
    {
        try
        {
            Logger.LogMessage("Initializing...");

            SingletonInitializer singletonInitializer = new(Logger);
            singletonInitializer.InitializeInGameSingleton();

            PatchMethods();
            Logger.LogMessage("The mod has been successfully initialized.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize the mod: {ex}");
            throw;
        }
    }

    private void PatchMethods()
    {
        try
        {
            Logger.LogInfo("[Harmony] Patching methods...");
            Harmony harmony = new Harmony(ModConstants.Id);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in AccessTools.GetTypesFromAssembly(assembly))
            {
                PatchClassProcessor processor = harmony.CreateClassProcessor(type);
                if (processor.Patch()?.Count > 0)
                    Logger.LogInfo($"[Harmony] {type.Name} successfully applied.");
            }
            // AccessTools.GetTypesFromAssembly(assembly).Do(type => harmony.CreateClassProcessor(type).Patch());
            // //public void PatchAll(Assembly assembly) => ((IEnumerable<Type>) AccessTools.GetTypesFromAssembly(assembly)).Do<Type>((Action<Type>) (type => this.CreateClassProcessor(type).Patch()));
            // harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to patch methods.", ex);
        }
    }
}