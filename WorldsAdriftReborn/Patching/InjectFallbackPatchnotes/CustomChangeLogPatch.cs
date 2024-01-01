using Bossa.Travellers.UI;
using HarmonyLib;

namespace WorldsAdriftReborn.Patching.InjectFallbackPatchnotes
{
    [HarmonyPatch(typeof(ChangeLogLoader))]
    internal class CustomChangeLogPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChangeLogLoader), "Start")]
        public static bool Start_Prefix(ChangeLogLoader __instance)
        {
            var parser = AccessTools.Method(typeof(ChangeLogLoader), "ParsePatchNotes");
            var patchNote1 = "<size=14>Worlds Adrift Reborn|01.01.2024</size><color=#bf9d82></color><size=11>A basic 2024 release that contains: </size>\nThe player!\nIslands!\nItems!\nThats it!\n<size=11>Keep an eye on the GitHub for updates - https://github.com/sp00ktober/WorldsAdriftReborn</size>";
            var patchNote2 = "<size=14>Ores & Tree Update|Coming Soon</size><color=#bf9d82></color><size=11>An upcoming release that contains: </size>\nWorking ore\nWorking trees\n<size=11>Come back in 2030</size>";
            parser.Invoke(__instance, new object[]
            {
                // Notes that come first appear at the top
                patchNote2 + patchNote1
            });
            return false;
        }
    }
}
