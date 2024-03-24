using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{

    [HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
    public static class GenConstruct_CanPlaceBlueprintOver_Patch
    {
        public static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null, Thing thing = null, ThingDef stuffDef = null)
        {
            CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
            foreach (IntVec3 cell in cellRect)
            {
                var thingList = cell.GetThingList(map);
                for (int m = 0; m < thingList.Count; m++)
                {
                    Thing thing2 = thingList[m];
                    var otherDef = thing2.def.IsBlueprint ? thing2.def.entityDefToBuild : thing2.def;
                    if (thing2 != thingToIgnore && otherDef.GetModExtension<CannotBePlacedTogetherWithThisModExtension>() != null
                        && entDef.GetModExtension<CannotBePlacedTogetherWithThisModExtension>() != null)
                    {
                        __result = new AcceptanceReport("SpaceAlreadyOccupied".Translate());
                        return;
                    }
                }
            }
        }
    }
}