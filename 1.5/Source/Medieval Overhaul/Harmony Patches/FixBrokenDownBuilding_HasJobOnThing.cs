using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(WorkGiver_FixBrokenDownBuilding), nameof(WorkGiver_FixBrokenDownBuilding.HasJobOnThing))]
    
    public static class FixBrokenDownBuilding_HasJobOnThing
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                                                       ILGenerator generator)
        {
            List<CodeInstruction> code = instructions.ToList();
            bool foundInjection = false;
            for (int i = 0; i < code.Count; i++)
            {
                // Look for the old Method
                if (code[i].opcode == OpCodes.Call &&
                    (MethodInfo)code[i].operand == HarmonyLib.AccessTools
                                 .Method(typeof(WorkGiver_FixBrokenDownBuilding), "FindClosestComponent"))
                {
                    foundInjection = true;
                    code.RemoveAt(i); // Removes old method
                    code.Insert(i,
                        new CodeInstruction(OpCodes.Ldarg_2)); // Inserts Thing into the method

                    code.Insert(i+1, // Inserts method
                                new CodeInstruction(OpCodes.Call, AccessTools
                                 .Method(typeof(Breakdown), "FindComponent",
                                           null)));
                    break;
                }
            }
            if (!foundInjection)
            {
                Log.Error("MO.WorkGiver_FixBrokenDownBuilding.HasJobOnThing: Could not find Harmony injection site");
            }
            foreach (var c in code) yield return c;
        }
    }
}
