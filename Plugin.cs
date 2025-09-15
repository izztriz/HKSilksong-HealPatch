using BepInEx;
using BepInEx.Logging;
using System;
using HarmonyLib;
using System.Linq;


namespace HealPatch;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInProcess("Hollow Knight Silksong.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public const string PLUGIN_GUID = "Plugin.HealPatch";
    public const string PLUGIN_NAME = "HealPatch";
    public const string PLUGIN_VERSION = "1.0.0";
    private readonly Harmony harmony = new Harmony(PLUGIN_GUID);
    public static float silkMultiplier = 1.2f;
    
    private void Awake()
    {
        Logger = base.Logger;
        silkMultiplier = Config.Bind("General", "SilkMultiplier", 1.2f, "Silk amount multiplier").Value;
        Logger.LogInfo($"✅ {PLUGIN_GUID} is loaded!");
        
        try
        {
            harmony.PatchAll();
            var patchedMethods = harmony.GetPatchedMethods().ToList();
            if (patchedMethods.Any())
            {
                Logger.LogInfo("✅ The following methods were patched:");
                foreach (var method in patchedMethods)
                {
                    Logger.LogInfo($"✅ {method.DeclaringType?.Name}.{method.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"❌ Error applying patches: {ex}");
        }
    }
}


