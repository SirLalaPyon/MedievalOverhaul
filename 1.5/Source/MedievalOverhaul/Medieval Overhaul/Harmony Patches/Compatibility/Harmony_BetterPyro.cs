using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using System;
using Verse.AI;

namespace MedievalOverhaul
{
    public class Harmony_BetterPyro
    {
        private static Type BetterPyro_JobDriver_WatchFlame
        {
            get
            {
                return AccessTools.TypeByName("BetterPyromania.JobDriver_WatchFlame");
            }
        }
        [HarmonyPatch]
        public class Harmony_Patch_BetterPyro
        {
            public static bool Prepare()
            {
                return BetterPyro_JobDriver_WatchFlame != null;
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("BetterPyromania.JobDriver_WatchFlame:WatchTickAction");
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var code = new List<CodeInstruction>(instructions);
                //var targetMethod = AccessTools.Method(typeof(ThingCompUtility),"TryGetComp",new[] { typeof(Thing) } ).MakeGenericMethod(typeof(CompRefuelable));
                var customMethod = AccessTools.Method(typeof(ThingCompUtility),"TryGetComp",new[] { typeof(Thing) } ).MakeGenericMethod(typeof(CompRefuelableCustom));
                var jobMethod = AccessTools.Method(typeof(JobDriver), "EndJobWith", new[] { typeof(JobCondition) });
               // var hasFuelMethod = AccessTools.PropertyGetter(typeof(CompRefuelable), "HasFuel");
                var hasFuelCustomMethod = AccessTools.PropertyGetter(typeof(CompRefuelableCustom), "HasFuel");
                Label endLabel = generator.DefineLabel();
                Label skipLabel = generator.DefineLabel();
                bool foundInjection = false;
                for (int j = 0; j < code.Count; j++)
                {
                    if (code[j].opcode == OpCodes.Brfalse_S)
                    {
                        code[j].operand = skipLabel;
                    }
                    if (code[j].opcode == OpCodes.Brtrue_S)
                    {
                        code[j].operand = skipLabel;
                    }
                    if (code[j].opcode == OpCodes.Ldarg_0 && code[j].labels.Count > 0)
                    {
                        code[j].labels.Add(endLabel);
                    }
                }
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ret && code[i + 1].opcode == OpCodes.Ldarg_0)
                    {
                        foundInjection = true;
                        code.InsertRange(i + 1, new[]
                        {
                             new CodeInstruction(OpCodes.Ldloc_0).WithLabels(skipLabel),
                             new CodeInstruction(OpCodes.Call, customMethod),
                             new CodeInstruction(OpCodes.Brfalse_S, endLabel),
                             new CodeInstruction(OpCodes.Ldloc_0),
                             new CodeInstruction(OpCodes.Call, customMethod),
                             new CodeInstruction(OpCodes.Callvirt,hasFuelCustomMethod),
                             new CodeInstruction(OpCodes.Brtrue_S, endLabel),
                             new CodeInstruction(OpCodes.Ldarg_0),
                             new CodeInstruction(OpCodes.Ldc_I4_4),
                             new CodeInstruction(OpCodes.Call, jobMethod),
                             new CodeInstruction(OpCodes.Ret)
                        });
                        break;
                    }
                }
                  

                if (!foundInjection)
                {
                    Log.Error("MO.BetterPyro.JobDriver_WatchFlame.WatchTickAction: Could not find Harmony injection site");
                }

                foreach (var c in code)
                {
                    yield return c;
                }
            }
        
        
        
        }

    }
}
