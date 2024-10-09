using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using ProcessorFramework;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class RefuelablePatching
    {
        public static Dictionary<ThingDef, CompProperties> AllRefuelables = [];
        public static HashSet<string> AllWorkTable_Refuelables = [];
        public static HashSet<string> AllWorkTable_HeatPushRefuelables = [];
        public static Dictionary<ThingDef, ThingDef> RefuelableSeen = [];
        public static Dictionary<ThingDef, ThingDef> WorkbenchSeen = [];
        public static FuelPatchList FuelPatchList = DefDatabase<FuelPatchList>.GetNamed("FuelPatchList");

        static RefuelablePatching()
        {
            MakeRefuelableList();
            AddFuel(AllRefuelables);
            //ProcessRefuelableList();
        }
        public static void MakeRefuelableList()
        {
            foreach (ThingDef building in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Building).ToList())
            {

                if (!FuelPatchList.blackListRefuelable.Contains(building) && building.thingClass != null && building.comps != null)
                {
                    for (int i = 0; i < building.comps.Count; i++)
                    {
                        if (building.comps[i] is CompProperties_Refuelable compRefuel && compRefuel?.fuelFilter?.thingDefs?.Contains(ThingDefOf.WoodLog) == true)
                        {
                            AllRefuelables.Add(building, building.comps[i]);
                        }
                    }
                   
                    //if (!MedievalOverhaulSettings.settings.refuelableTorch)
                    //{

                    //    if (building.thingClass == typeof(Building))
                    //    {
                    //        for (int i = 0; i < building.comps.Count; i++)
                    //        {
                                
                    //        }

                    //    }
                    //    for (int i = 0; i < building.comps.Count; i++)
                    //    {
                    //        if (building.comps[i] != null && building.comps[i] is CompProperties_Power)
                    //        {
                    //            AllRefuelables.Remove(building);
                    //        }
                    //    }
                    //}
                    //if (building.thingClass == typeof(Building_WorkTable))
                    //{
                    //    for (int i = 0; i < building.comps.Count; i++)
                    //    {
                    //        if (building.comps[i] != null && building.comps[i] is CompProperties_Refuelable compRefuel && compRefuel?.fuelFilter?.thingDefs?.Contains(ThingDefOf.WoodLog) == true)
                    //        {

                    //            AllRefuelables.Add(building);
                    //            AllWorkTable_Refuelables.Add(building.defName.ToString());
                    //        }
                    //    }

                    //    for (int i = 0; i < building.comps.Count; i++)
                    //    {
                    //        if (building.comps[i] != null && building.comps[i] is CompProperties_Power)
                    //        {
                    //            AllRefuelables.Remove(building);
                    //            AllWorkTable_Refuelables.Remove(building.defName.ToString());
                    //        }
                    //        if (building.comps[i] != null && building.comps[i] is CompProperties_Processor)
                    //        {
                    //            AllRefuelables.Remove(building);
                    //            AllWorkTable_Refuelables.Remove(building.defName.ToString());
                    //        }
                    //    }
                    //}
                    //if (building.thingClass == typeof(Building_WorkTable_HeatPush))
                    //{
                    //    foreach (CompProperties comp in building.comps)
                    //    {
                    //        if (comp is CompProperties_Refuelable compRefuel && compRefuel?.fuelFilter?.thingDefs?.Contains(ThingDefOf.WoodLog) == true)
                    //        {
                    //            AllRefuelables.Add(building);
                    //            AllWorkTable_HeatPushRefuelables.Add(building.defName);
                    //        }
                    //    }
                    //    foreach (CompProperties comp in building.comps)
                    //    {
                    //        if (comp is CompProperties_Power compRefuel)
                    //        {
                    //            AllRefuelables.Remove(building);
                    //            AllWorkTable_HeatPushRefuelables.Remove(building.defName);
                    //        }
                    //    }
                    //}
                }
            }
        }

        public static void AddFuel (Dictionary<ThingDef, CompProperties> refuelableThings)
        {
            foreach(var thing in refuelableThings)
            {

            }
        }

        public static void ProcessRefuelableList (HashSet<ThingDef> buildingList)
        {
            foreach(ThingDef building in buildingList)
            {
                
                for (int i = 0; i < building.comps.Count; i++)
                {
                    var comp = building.comps[i];
                    if (comp is CompProperties_Refuelable compRefuel && compRefuel.fuelFilter.thingDefs.Contains(ThingDefOf.WoodLog))
                    {

                        building.comps.Add(new CompProperties_RefuelableCustom()
                        {
                            fuelConsumptionRate = compRefuel.fuelConsumptionRate,
                            fuelCapacity = compRefuel.fuelCapacity,
                            fuelFilter = compRefuel.fuelFilter,
                            consumeFuelOnlyWhenUsed = compRefuel.consumeFuelOnlyWhenUsed,
                            showAllowAutoRefuelToggle = compRefuel.showAllowAutoRefuelToggle,
                            fuelIconPath = compRefuel.fuelIconPath,
                            initialFuelPercent = compRefuel.initialFuelPercent,
                            autoRefuelPercent = compRefuel.autoRefuelPercent,
                            fuelConsumptionPerTickInRain = compRefuel.fuelConsumptionPerTickInRain,
                            destroyOnNoFuel = compRefuel.destroyOnNoFuel,
                            consumeFuelOnlyWhenPowered = compRefuel.consumeFuelOnlyWhenPowered,
                            showFuelGizmo = compRefuel.showFuelGizmo,
                            initialAllowAutoRefuel = compRefuel.initialAllowAutoRefuel,
                            allowRefuelIfNotEmpty = compRefuel.allowRefuelIfNotEmpty,
                            fuelIsMortarBarrel = compRefuel.fuelIsMortarBarrel,
                            targetFuelLevelConfigurable = compRefuel.targetFuelLevelConfigurable,
                            initialConfigurableTargetFuelLevel = compRefuel.initialConfigurableTargetFuelLevel,
                            drawOutOfFuelOverlay = compRefuel.drawOutOfFuelOverlay,
                            minimumFueledThreshold = compRefuel.minimumFueledThreshold,
                            drawFuelGaugeInMap = compRefuel.drawFuelGaugeInMap,
                            atomicFueling = compRefuel.atomicFueling,
                            factorByDifficulty = compRefuel.factorByDifficulty,
                            fuelLabel = compRefuel.fuelLabel,
                            fuelGizmoLabel = compRefuel.fuelGizmoLabel,
                            outOfFuelMessage = compRefuel.outOfFuelMessage,
                            externalTicking = compRefuel.externalTicking,

                        });
                        if (building.comps[building.comps.Count -1] is CompProperties_Refuelable compRefuelableCustom)
                        {
                            if (compRefuelableCustom.fuelFilter.categories == null)
                            {
                                compRefuelableCustom.fuelFilter.categories = new List<string>
                                {
                                    "DankPyon_RawWood",
                                    "DankPyon_DarkWood"
                                };
                            }
                            compRefuelableCustom.fuelFilter.thingDefs.Add(MedievalOverhaulDefOf.DankPyon_Coal);
                            compRefuelableCustom.fuelFilter.thingDefs.Add(MedievalOverhaulDefOf.DankPyon_Tar);
                        }    
                        if (building.inspectorTabsResolved == null)
                        {

                        }
                        if (building.inspectorTabsResolved == null)
                        {
                            building.inspectorTabsResolved = new List<InspectTabBase>();
                        }

                        // Add custom inspector tab only if it doesn't already exist
                        if (!building.inspectorTabsResolved.Any(tab => tab is ITab_FuelCustom))
                        {
                            building.inspectorTabsResolved.AddDistinct(new MedievalOverhaul.ITab_FuelCustom());
                        }

                        building.comps.Remove(compRefuel);
                    }
                    if (comp is CompProperties_FireOverlay compFireOverlay)
                    {
                        building.comps.Add(new CompProperties_FireOverlayCustom()
                        { 
                            fireSize = compFireOverlay.fireSize,
                            finalFireSize = compFireOverlay.finalFireSize,
                            fireGrowthDurationTicks = compFireOverlay.fireGrowthDurationTicks,
                            offset = compFireOverlay.offset,
                            offsetEast = compFireOverlay.offsetEast,
                            offsetWest = compFireOverlay.offsetWest,
                            offsetNorth = compFireOverlay.offsetNorth,
                            offsetSouth = compFireOverlay.offsetSouth,
                        });
                        building.comps.Remove(compFireOverlay);
                    }
                    if (comp is CompProperties_HeatPusher compHeaterPusher)
                    {
                        if (compHeaterPusher.compClass == typeof(CompHeatPusherPowered))
                        {
                            compHeaterPusher.compClass = typeof(CompHeatPusherPoweredCustom);
                        }
                    }
                }
                if (building.thingClass == typeof(Building_WorkTable))
                {
                    building.thingClass = typeof(Building_WorkTableCustom);
                }
                if (building.thingClass == typeof(Building_WorkTable_HeatPush))
                {
                    building.thingClass = typeof(Building_WorkTable_HeatPushCustom);
                }
                Log.Message("Sucesfully patched" + " " + building);
            }
        }

    }
}
