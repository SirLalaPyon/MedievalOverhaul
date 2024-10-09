using System;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class Utility
    {
        public static SeperateHideList WhiteList = DefDatabase<SeperateHideList>.GetNamed("WhiteList");
        public static SeperateWoodList LogList = DefDatabase<SeperateWoodList>.GetNamed("LogList");
        public static HideGraphicList HideGraphicList = DefDatabase<HideGraphicList>.GetNamed("HideGraphicList");
        public static ModContentPack myContentPack = LoadedModManager.GetMod<MedievalOverhaulSettings>().Content;
        public static bool CEIsEnabled = LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId == "ceteam.combatextended"));
        public static bool VBEIsEnabled = ModsConfig.IsActive("VanillaExpanded.VBooksE");
        public static bool LWMFuelFilterIsEnabled = ModsConfig.IsActive("lwm.fuelfilter");
        public static RoomRoleDef DankPyon_Library;
        public static ThoughtDef DankPyon_ReadInLibrary;
       

        
        static Utility()
        {
            if (VBEIsEnabled is false)
            {
                DankPyon_Library = DefDatabase<RoomRoleDef>.GetNamed("DankPyon_Library");
                DankPyon_ReadInLibrary = DefDatabase<ThoughtDef>.GetNamed("DankPyon_ReadInLibrary");
            }
        }
        public static int GetFuelCountToFullyRefuel(CompRefuelable RefuelableComp)
        {
            if (RefuelableComp.Props.atomicFueling)
            {
                return Mathf.CeilToInt(RefuelableComp.Props.fuelCapacity / RefuelableComp.Props.FuelMultiplierCurrentDifficulty);
            }
            return Mathf.Max(Mathf.CeilToInt((RefuelableComp.TargetFuelLevel - RefuelableComp.fuel) / RefuelableComp.Props.FuelMultiplierCurrentDifficulty), 1);
        }
        public static int GetFuelCountToFullyRefuel(CompRefuelable RefuelableComp, Thing fuelThing)
        {
            float fuelValue = CachingUtility.FuelValueDict.GetValueOrDefault(fuelThing.def, 1f);
            return Mathf.CeilToInt(GetFuelCountToFullyRefuel(RefuelableComp) / fuelValue);
        }

        public static Thing FindSpecificFuel(Pawn pawn, ThingDef fuel)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(fuel), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: new Predicate<Thing>(validator));

            bool validator(Thing x) => !ForbidUtility.IsForbidden(x, pawn) && pawn.CanReserve((LocalTargetInfo)x);
        }

        public static Thing FindSpecificClosestFuel(Pawn pawn, List<ThingDef> fuelDefList)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: new Predicate<Thing>(validator));

            bool validator(Thing x) => !ForbidUtility.IsForbidden(x, pawn) && fuelDefList.Contains(x.def) && pawn.CanReserve((LocalTargetInfo)x);
        }
        public static bool FilterItemExists(ThingFilter filter, Pawn pawn)
        {
            foreach (var def in filter.AllowedThingDefs)
            {
                List<Thing> thingsOfDef = pawn.Map.listerThings.ThingsOfDef(def);
                if (thingsOfDef.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static string RemoveSubstring(ThingDef thingDef, string partToRemove)
        {
            string stringDefName = thingDef.defName;
            int index = stringDefName.IndexOf(partToRemove);
            if (index == -1)
            {
                return stringDefName;
            }
            else
            {
                string modifiedString = stringDefName.Replace(partToRemove, "");

                if (IsValidString(modifiedString))
                {
                    return modifiedString;
                }
                else
                {
                    return stringDefName;
                }
            }
        }
        static bool IsValidString(string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        public static Mineable_CompSpawnerDestroy GetFirstMineable(this IntVec3 c, Map map)
        {
            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] is Mineable_CompSpawnerDestroy mineable)
                {
                    return mineable;
                }
            }
            return null;
        }
    }
}
