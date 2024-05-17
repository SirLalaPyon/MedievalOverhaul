using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ThingSetMaker_Meteorite), "Reset")]
    public static class ThingSetMaker_Meteorite_Reset
    {
        public static void Postfix()
        {
            List<ThingDef> thingDefs = new List<ThingDef>();
            for (int i = 0; i < ThingSetMaker_Meteorite.nonSmoothedMineables.Count; i++)
            {
                ThingDef thing = ThingSetMaker_Meteorite.nonSmoothedMineables[i];
                if (thing.thingClass.Name.Equals("Mineable_CompSpawnerDestroy"))
                {
                    thingDefs.Add(thing);
                }
            }
            for (int t = 0; t < thingDefs.Count; t++)
            {
                if (ThingSetMaker_Meteorite.nonSmoothedMineables.Contains(thingDefs[t]))
                {
                    ThingSetMaker_Meteorite.nonSmoothedMineables.Remove(thingDefs[t]);
                }
            }
            thingDefs.Clear();
        }
    }
}
