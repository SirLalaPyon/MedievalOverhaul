using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(JobDriver_Refuel), nameof(JobDriver_Refuel.MakeNewToils))]
    public class JobDriver_Refuel_MakeNewToils_Patch
    {
        private static IEnumerable<Toil> Postfix(IEnumerable<Toil> steps, JobDriver_Refuel __instance)
        {
            foreach (Toil toil in steps)
            {
               
                if (toil.ToString() == "DoAtomic")
                {
                    toil.Clear();
                    Toil newToil = Toils_General.DoAtomic(delegate
                    {
                        __instance.job.count = Utility.GetFuelCountToFullyRefuel(__instance.RefuelableComp, __instance.Fuel);
                    });
                    yield return newToil;
                    
                }
                yield return toil;
            }
        }

    }
}