﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Bossa.Travellers.Inventory;
using Bossa.Travellers.Misc;
using Bossa.Travellers.Player;
using Bossa.Travellers.Scanning;
using Bossa.Travellers.Utils;
using Bossa.Travellers.Visualisers.Profile;
using HarmonyLib;
using Improbable;
using Improbable.Collections;
using Improbable.Entity.Component;
using Improbable.Unity.Internal;
using Improbable.Unity.Visualizer;
using Improbable.Util.Injection;
using Improbable.Worker;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

/*
 * this is just to get a list of Component IDs mapped to their respective component type
 */
namespace WorldsAdriftReborn.Patching.SpatialOS.Debug_ComponentFactory
{
    [HarmonyPatch()]
    internal class DispatchEventHandler_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                                        AccessTools.TypeByName("DispatchEventHandler"),
                                        "RegisterComponentFactories");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RegisterComponentFactories_Transpiler(IEnumerable<CodeInstruction> instructions )
        {
            return new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RegisterWithConnection"))
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_2),
                    Transpilers.EmitDelegate<Func<IComponentFactory, int>>(( componentFactory ) =>
                    {
                        Debug.LogWarning(componentFactory.ComponentId + " => " + componentFactory);
                        return 0;
                    }),
                    new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
        }
    }
}
