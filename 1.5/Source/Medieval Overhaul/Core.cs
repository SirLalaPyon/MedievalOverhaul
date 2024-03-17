using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace DankPyon
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            var gotoCastPositionMethod = typeof(Toils_Combat).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredMethods(innerType))
                .FirstOrDefault(method => method.Name.Contains("<GotoCastPosition>") && method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
            HarmonyInstance.harmony.Patch(gotoCastPositionMethod, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Core), nameof(GotoCastPositionTranspiler))));

            var tryFindShootLineFromToMethod = AccessTools.Method(typeof(Verb), "TryFindShootLineFromTo");
            HarmonyInstance.harmony.Patch(tryFindShootLineFromToMethod, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Core), nameof(TryFindShootLineFromToTranspiler))));
        }

        public static IEnumerable<CodeInstruction> GotoCastPositionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(ShootTuning.MeleeRange))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Job), "verbToUse"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(GetMeleeReachRange)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static IEnumerable<CodeInstruction> TryFindShootLineFromToTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label = generator.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(AccessTools.Method(typeof(VerbProperties), "get_IsMeleeAttack")))
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(IsVanillaMeleeAttack)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static bool IsVanillaMeleeAttack(Verb verb)
        {
            if (MedievalOverhaulMod.settings.enableMultiTileMeleeAttacks && verb.Caster is Pawn pawn && pawn.GetMeleeReachRange(verb) > ShootTuning.MeleeRange)
            {
                return false;
            }
            return true;
        }

        public static float GetMeleeReachRange(this Pawn caster, Verb verb)
        {
            if (verb?.tool is Tool_Melee tool)
            {
                return tool.meleeReachRange;
            }
            return ShootTuning.MeleeRange;
        }
    }
}
