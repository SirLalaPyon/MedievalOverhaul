using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class RefuelablePatching
    {
        public static HashSet<ThingDef> AllRefuelables = [];
        public static HashSet<ThingDef> AllNonWorkbenchRefuelables = [];
        public static HashSet<string> AllWorkTable_Refuelables = [];
        public static HashSet<string> AllWorkTable_HeatPushRefuelables = [];
        public static Dictionary<ThingDef, ThingDef> RefuelableSeen = [];
        public static Dictionary<ThingDef, ThingDef> WorkbenchSeen = [];

        static RefuelablePatching()
        {
            MakeRefuelableList();
            ProcessRefuelableList(AllRefuelables);
        }
        public static void MakeRefuelableList()
        {
            foreach (ThingDef building in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Building).ToList())
            {

                if (building.thingClass != null && building.comps != null)
                {
                    if (!MedievalOverhaulSettings.settings.refuelableTorch)
                    {
                        if (building.thingClass == typeof(Building))
                        {
                            foreach (CompProperties comp in building.comps)
                            {
                                if (comp is CompProperties_Refuelable compRefuel && compRefuel.fuelFilter.thingDefs.Contains(ThingDefOf.WoodLog))
                                {
                                    AllRefuelables.Add(building);
                                    AllNonWorkbenchRefuelables.Add(building);
                                }

                            }
                            foreach (CompProperties comp in building.comps)
                            {
                                if (comp is CompProperties_Power compRefuel)
                                {
                                    AllRefuelables.Remove(building);
                                    AllNonWorkbenchRefuelables.Remove(building);
                                }
                            }
                            if (building.defName == "PassiveCooler")
                            {
                                AllRefuelables.Remove(building);
                            }

                        }
                    }
                
                    //if (building.thingClass == typeof(Building_WorkTable))
                    //{
                    //    foreach (CompProperties comp in building.comps)
                    //    {
                    //        if (comp is CompProperties_Refuelable compRefuel && compRefuel.fuelFilter.thingDefs.Contains(ThingDefOf.WoodLog))
                    //        {
                    //            AllRefuelables.Add(building);
                    //            AllWorkTable_Refuelables.Add(building.defName.ToString());
                    //        }

                    //    }
                    //    foreach (CompProperties comp in building.comps)
                    //    {
                    //        if (comp is CompProperties_Power compRefuel)
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
                    //        if (comp is CompProperties_Refuelable compRefuel && compRefuel.fuelFilter.thingDefs.Contains(ThingDefOf.WoodLog))
                    //        {
                    //            //AllRefuelables.Add(building);
                    //            AllWorkTable_HeatPushRefuelables.Add(building.defName);
                    //        }
                    //    }
                    //    foreach (CompProperties comp in building.comps)
                    //    {
                    //        if (comp is CompProperties_Power compRefuel)
                    //        {
                    //           // AllRefuelables.Remove(building);
                    //            AllWorkTable_HeatPushRefuelables.Remove(building.defName);
                    //        }
                    //    }
                    //}
                }
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
                //if (building.thingClass.ToString() == "Verse.Building_WorkTable_Heat")
                //{
                //    building.thingClass = typeof(Building_WorkTableCustom);
                //}
                //if (building.thingClass.ToString() == "Verse.Building_WorkTable_HeatPush")
                //{
                //    building.thingClass = typeof(Building_WorkTable_HeatPushCustom);
                //}
                    //building.inspectorTabsResolved.Add(new ITab_FuelCustom());
                    Log.Message("Sucesfully patched" + " " + building);
            }
        }

    }
}
