using System;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit; // for OpCodes in Harmony Transpiler
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace LWM.FuelFilter
{
    [HarmonyPatch(typeof(RimWorld.RefuelWorkGiverUtility), "FindBestFuel")]
    public static class Patch_RefuelWorkGiverUtility_FindBestFuel {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                                                       ILGenerator generator) {
            List<CodeInstruction> code=instructions.ToList();
            // Replace
            //       ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
            // with
            //       ThingFilter filter = refuelable.TryGetComp<Comp_FuelFilter>().filter;
            bool foundInjection=false;
            for (int i=0; i<code.Count; i++) {
                // Look for the .Props, because that's easier to find:
                if (code[i].opcode == OpCodes.Callvirt &&
                    (MethodInfo)code[i].operand == HarmonyLib.AccessTools
                                 .Method(typeof(RimWorld.CompRefuelable), "get_Props")) {
                    foundInjection = true;
                    code.RemoveAt(i - 1); // TryGetComp<CompRefuelable>()
                    code.RemoveAt(i - 1); // .Props
                    code.RemoveAt(i - 1); // .fuelFilter

                    // TryGetComp<Comp_FuelFilter>() - I hope I am using the last parameter of
                    //     HarmonyLib's AccessTools's Method correctly!  If so, this is much 
                    //     easier than earlier approaches!  (Spoiler - I am :D )
                    code.Insert(i - 1, new CodeInstruction(OpCodes.Call, AccessTools
                                 .Method(typeof(Verse.ThingCompUtility), "TryGetComp", null,
                                           new Type[] { typeof(Comp_FuelFilter) })));
                    code.Insert(i, // after i-1, add .filter
                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(
                                       typeof(Comp_FuelFilter), "filter")));
                    break;
                }
            }
            if (!foundInjection) {
                Log.Error("LWM.FuelFilter: could not find Harmony injection site");
            }
            foreach (var c in code) yield return c;
        }

    }


}
