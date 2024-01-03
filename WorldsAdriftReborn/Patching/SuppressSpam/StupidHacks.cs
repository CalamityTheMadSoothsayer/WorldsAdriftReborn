using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using WASystems.Components.Weather;

namespace WorldsAdriftReborn.Patching.SuppressSpam
{
    // [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.Respawn))]
    // public static class RespawnHack
    // {
    //     public static void Prefix( PlayerMove __instance )
    //     {
    //         try
    //         {
    //             var puppet = UnityEngine.Object.FindObjectsOfType<KnockOutPuppet>().First(p => p.isActiveAndEnabled);
    //             AccessTools.Field(typeof(PlayerMove), "knockoutPuppet").SetValue(__instance, puppet);
    //         }
    //         catch (InvalidOperationException)
    //         {
    //             UnityEngine.Debug.LogWarning("Unable to patch PlayerMove.Respawn!");
    //         }
    //     }
    // }
    
    // Terrible code but I'm sick of seeing this in my console
    [HarmonyPatch(typeof(ChararacterDrunk), "SetDrunkLevel")]
    public static class PuppetHack
    {
        private static bool magicComplete = false;
        private static KnockOutPuppet puppet;

        [HarmonyPrefix]
        public static void Prefix(ChararacterDrunk __instance)
        {
            if (magicComplete) return;
            if (LocalPlayer.Instance.playerKnockout != null) return;
            magicComplete = true;
            try
            {
                puppet = UnityEngine.Object.FindObjectsOfType<KnockOutPuppet>().First(p => p.isActiveAndEnabled);
            }
            catch (InvalidOperationException)
            {
                UnityEngine.Debug.LogWarning("Unable to patch ChararacterDrunk.SetDrunkValue!");
                return;
            }

            var vField = AccessTools.Field(typeof(LocalPlayer), "_visualizers");
            var vReal = vField.GetValue(LocalPlayer.Instance);
            var kField = AccessTools.Field(vReal.GetType(), "playerKnockout");
            kField.SetValue(vReal, puppet);
            magicComplete = true;
        }
    }
    
    // Terrible code but I'm sick of seeing this in my console
    [HarmonyPatch]
    class StupidHacks
    {
        private static Type WeatherCellType;
        
        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            WeatherCellType = AccessTools.AllTypes().First(n => n.Name.Contains("AddToIdC")).MakeGenericType(typeof(WeatherCellCoordsC), typeof(uint));
            
            if (WeatherCellType == null)
                throw new Exception("Could not find type");

            var method = AccessTools.Method(WeatherCellType, "Execute");
            if (method == null)
                throw new Exception("Could not find method");

            return method;
        }

        public static void SuppressWarning<T>(string msg)
        {
            return; // We do not care
        }

        [HarmonyTranspiler, UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpile( IEnumerable<CodeInstruction> originalInstructions )
        {
            foreach (var ci in originalInstructions)
            {
                if (ci.opcode == OpCodes.Call && ci.operand is MethodInfo info && (info.Name == "Warn" || info.Name == "Error"))
                {
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(StupidHacks), nameof(SuppressWarning)).MakeGenericMethod(WeatherCellType));
                }
                else
                {
                    yield return ci;
                }
            }
        }
    }
}
