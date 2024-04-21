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

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(WorkGiver_FixBrokenDownBuilding), nameof(WorkGiver_FixBrokenDownBuilding.JobOnThing))]

    public static class FixBrokenDownBuilding_JobOnThing_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> code = instructions.ToList();
            bool foundInjection = false;
            for (int i = 0; i < code.Count; i++)
            {
                // Look for the old Method
                if (code[i].opcode == OpCodes.Call && 
                    (MethodInfo)code[i].operand == AccessTools.Method(typeof(WorkGiver_FixBrokenDownBuilding), "FindClosestComponent"))
                {
                    foundInjection = true;
                    // Removes old method
                    code.RemoveAt(i);
                    // Passes Thing into the method
                    code.Insert(i, new CodeInstruction(OpCodes.Ldarg_2)); 
                    // Inserts new method
                    code.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Breakdown), "FindComponent", null))); 
                    break;
                }
            }
            if (!foundInjection)
            {
                Log.Error("MO.WorkGiver_FixBrokenDownBuilding.JobOnThing: Could not find Harmony injection site");
            }
            foreach (var c in code) yield return c;
        }
    }
}
