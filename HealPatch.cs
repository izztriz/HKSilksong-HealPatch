using HarmonyLib;
using System;

namespace HealPatch
{
    [HarmonyPatch]
    public class HealPatch
    {
        private static bool isHealingInProgress = false;
        private static int missingHealth = 0;
        private static int lastSilkValue = 0;
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), "AddSilk")]
        private static void AddSilkPrefix(PlayerData __instance, int amount) => ProcessSilkChange(__instance, amount);
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), "TakeSilk")]
        private static void TakeSilkPrefix(PlayerData __instance, int amount) => ProcessSilkChange(__instance, -amount);
        
        private static void ProcessSilkChange(PlayerData instance, int amount)
        {
            try
            {
                if (instance == null) return;
                
                int currentSilk = instance.silk;
                int modifiedAmount = amount > 0 ? (int)Math.Round(amount * Plugin.silkMultiplier) : amount;
                int newSilk = currentSilk + modifiedAmount;
                
                newSilk = Math.Min(instance.silkMax, Math.Max(0, newSilk));
                
                instance.silk = newSilk;
                
                if (amount > 0) 
                {
                    lastSilkValue = newSilk;
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), "AddHealth")]
        private static bool AddHealthPrefix(PlayerData __instance, ref int amount)
        {
            try
            {
                if (isHealingInProgress) return true;
                
                isHealingInProgress = true;
                
                int currentMaxHealth = __instance.maxHealth;
                missingHealth = currentMaxHealth - __instance.health;
                
                amount = missingHealth;
                return true; 
            }
            catch (Exception ex)
            {
                isHealingInProgress = false;
                return false;
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), "AddHealth")]
        private static void AddHealthPostfix(PlayerData __instance)
        { 
            try
            {
                if (missingHealth <= 0) return;
                
                int silkAfterHeal = lastSilkValue - missingHealth;
                
                silkAfterHeal = Math.Max(0, Math.Min(silkAfterHeal, __instance.silkMax));
                
                __instance.silk = silkAfterHeal;
                lastSilkValue = silkAfterHeal;
                
                EventRegister.SendEvent(EventRegisterEvents.SilkCursedUpdate);
                
                EventRegister.SendEvent(EventRegisterEvents.HealthUpdate);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                missingHealth = 0;
                isHealingInProgress = false;
            }
        }
    }
}