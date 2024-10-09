using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Steamworks;
using UnityEngine;
using Verse.AI.Group;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.Refuel), new[] { typeof(List<Thing>) })]
    public class CompRefuelable_Refuel
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            bool foundPop = false;
            bool foundSub = false;
            int popIndex = -1;
            int subIndex = -1;
            var targetMethod = AccessTools.Method(typeof(GenCollection), "Pop").MakeGenericMethod(typeof(Thing));
            for (int i = 0; i < code.Count; i++)
            {
                if (!foundPop && code[i].opcode == OpCodes.Call && (MethodInfo)code[i].operand == targetMethod)
                {
                    foundPop = true;
                    popIndex = i;
                }
                else if (foundPop && code[i].opcode == OpCodes.Sub)
                {
                    foundSub = true;
                    subIndex = i;
                    break;
                }
            }
            if (!foundPop || !foundSub)
            {
                Log.Error("MO.CompRefuelable.Refuel: Could not find injection site");
            }
            else
            {
                code.RemoveRange(popIndex, subIndex - popIndex + 1);

                code.Insert(popIndex, new CodeInstruction(OpCodes.Ldarg_1));
                code.Insert(popIndex + 1, new CodeInstruction(OpCodes.Ldloc_0));
                code.Insert(popIndex + 2, new CodeInstruction(OpCodes.Ldarg_0));
                code.Insert(popIndex + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CompRefuelable_Refuel), "RefuelValue")));
            }
            foreach (var c in code)
            {
                //Log.Message(c.ToString());
                yield return c;
            }
        }
        public int RefuelValue (List<Thing> fuelThing, int fullFuelCount, CompRefuelable instance)
        {
            Thing thing = fuelThing.Pop<Thing>();
            float fuelValue = thing.def?.GetModExtension<FuelValueProperty>()?.fuelValue ?? 1f;
            int maxFuelNeededFromStack = Mathf.CeilToInt(fullFuelCount / fuelValue);
            int amountToFuel = Mathf.Min(maxFuelNeededFromStack, thing.stackCount);
            instance.Refuel((float)amountToFuel * fuelValue);
            thing.SplitOff(amountToFuel).Destroy(DestroyMode.Vanish);
            fullFuelCount -= (int)(amountToFuel * fuelValue);
            return fullFuelCount;
        }
    }
}