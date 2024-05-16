using HarmonyLib;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(DamageWorker_Stun), nameof(DamageWorker_Stun.Apply))]
    public static class DamageWorker_Stun_Result
    {
        [HarmonyPostfix]
        public static void Postfix(ref DamageWorker.DamageResult __result, Thing victim)
        {
            if (__result != null)
            {
                Pawn pawn = victim as Pawn;
                if (pawn != null)
                {
                    if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_StunImmune))
                    {
                        __result.stunned = false;
                    }
                }
            }
        }
    }
}