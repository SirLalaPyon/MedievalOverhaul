using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(Quest), "End")]
    public static class Quest_End_Patch
    {
        public static void Postfix(Quest __instance, QuestEndOutcome outcome)
        {
            GameComponent_QuestFinder.Instance.Notify_QuestComplete(__instance, outcome);
        }
    }
}
