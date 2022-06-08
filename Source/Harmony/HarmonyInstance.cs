using HarmonyLib;
using ProcessorFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DankPyon
{
    public class CompGenericHide : ThingComp
    {
        public ThingDef pawnSource;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref pawnSource, "pawnSource");
        }
    }
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {
        public static Dictionary<Thing, PlantExtension> cachedTransparentablePlantsByExtensions = new Dictionary<Thing, PlantExtension>();
        public static Dictionary<Map, List<Thing>> cachedTransparentablePlantsByMaps = new Dictionary<Map, List<Thing>>();

        public static Dictionary<HediffDef, StatDef> statMultipliers = new Dictionary<HediffDef, StatDef>();

        public static Harmony harmony;
        static HarmonyInstance()
        {
            harmony = new Harmony("lalapyhon.rimworld.medievaloverhaul");
            harmony.Patch(AccessTools.Method(typeof(Pawn), "ButcherProducts", null, null), null,
                postfix: new HarmonyMethod(typeof(HarmonyInstance), nameof(MakeButcherProducts_Postfix), null), null);
            harmony.PatchAll();
            foreach (var stat in DefDatabase<StatDef>.AllDefs)
            {
                var extension = stat.GetModExtension<HediffFallRateMultiplier>();
                if (extension != null && extension.hediffDef != null)
                {
                    statMultipliers[extension.hediffDef] = stat;
                }
            }

            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.IsChunk() && def.projectileWhenLoaded is null)
                {
                    def.projectileWhenLoaded = DankPyonDefOf.DankPyon_Artillery_Boulder;
                }
            }

            foreach (var pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (pawnKindDef.RaceProps.Animal && pawnKindDef.race.race.leatherDef is null && pawnKindDef.lifeStages != null 
                    && pawnKindDef.lifeStages.Last().butcherBodyPart is null && !pawnKindDef.RaceProps.Dryad && !pawnKindDef.RaceProps.Insect)
                {
                    pawnKindDef.lifeStages.Last().butcherBodyPart = new BodyPartToDrop
                    {
                        bodyPartGroup = DankPyonDefOf.HeadAttackTool,
                        thing = DankPyonDefOf.DankPyon_Hide_HideGeneric,
                        allowFemale = true
                    };
                }
            }
        }

        public static bool IsChunk(this ThingDef def)
        {
            if (!def.thingCategories.NullOrEmpty())
            {
                if (!def.thingCategories.Contains(ThingCategoryDefOf.Chunks))
                {
                    return def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks);
                }
            }
            return false;
        }

        private static Dictionary<Thing, HashSet<IntVec3>> cachedCells = new Dictionary<Thing, HashSet<IntVec3>>();
        public static HashSet<IntVec3> GetTransparentCheckArea(this Thing plant, PlantExtension extension)
        {
            if (!cachedCells.TryGetValue(plant, out var cells))
            {
                cachedCells[plant] = cells = GetTransparentCheckAreaInt(plant, extension);
            }
            return cells;
        }
        private static HashSet<IntVec3> GetTransparentCheckAreaInt(Thing plant, PlantExtension extension)
        {
            var cellRect = new CellRect(plant.Position.x, plant.Position.z, extension.firstArea.x, extension.firstArea.z);
            if (extension.firstAreaOffset != IntVec2.Zero)
            {
                cellRect = cellRect.MovedBy(extension.firstAreaOffset);
            }
            var cells = cellRect.Cells.ToList();
            if (extension.secondArea != IntVec2.Zero)
            {
                var cells2 = new CellRect(plant.Position.x, plant.Position.z, extension.secondArea.x, extension.secondArea.z);
                if (extension.secondAreaOffset != IntVec2.Zero)
                {
                    cells2 = cells2.MovedBy(extension.secondAreaOffset);
                }
                cells.AddRange(cells2);
            }
            return cells.Where(x => x.InBounds(plant.Map)).ToHashSet();
        }
        public static bool HasItemsInCell(IntVec3 cell, Map map, PlantExtension extension)
        {
            foreach (var thing in cell.GetThingList(map))
            {
                if (ItemMatches(thing, extension))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool BaseItemMatches(Thing thing)
        {
            return thing is Pawn || thing.def.category == ThingCategory.Item;
        }
        public static bool ItemMatches(Thing thing, PlantExtension extension)
        {
            return (thing is Pawn || thing.def.category == ThingCategory.Item) && (extension.ignoredThings is null || !extension.ignoredThings.Contains(thing.def));
        }

        public static Dictionary<Thing, Shader> lastCachedShaders = new Dictionary<Thing, Shader>();
        public static void RecheckTransparency(Thing plant, Thing otherThing, PlantExtension extension)
        {
            if (plant != otherThing && plant.Spawned && plant.Map == otherThing.Map)
            {
                if (!lastCachedShaders.TryGetValue(plant, out var shader))
                {
                    lastCachedShaders[plant] = shader = plant.Graphic.Shader;
                }

                bool isTransparent = shader == DankPyonDefOf.TransparentPlant.Shader;
                if (!isTransparent && ItemMatches(otherThing, extension))
                {
                    var cells = GetTransparentCheckArea(plant, extension);
                    if (cells.Contains(otherThing.Position))
                    {
                        otherThing.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                        return;
                    }
                }

                if (isTransparent)
                {
                    var cells = GetTransparentCheckArea(plant, extension);
                    if (!cells.Any(x => HasItemsInCell(x, otherThing.Map, extension)))
                    {
                        otherThing.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GenConstruct), "CanBuildOnTerrain")]
        public static class GenConstruct_CanBuildOnTerrain_Patch
        {
            public static void Postfix(ref bool __result, BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null, ThingDef stuffDef = null)
            {
                if (entDef == DankPyonDefOf.DankPyon_PlowedSoil && !c.GetTerrain(map).IsSoil)
                {
                    __result = false;
                }
            }
        }

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

        [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
        public class SavedGameLoaderNow_LoadGameFromSaveFileNow
        {
            public static void Prefix()
            {
                cachedTransparentablePlantsByExtensions.Clear();
                cachedTransparentablePlantsByMaps.Clear();
            }
        }

        [HarmonyPatch(typeof(Plant), "SpawnSetup")]
        public class Thing_SpawnSetup_Patch
        {
            private static void Postfix(Thing __instance)
            {
                if (MedievalOverhaulMod.settings.enableTreeTransparency)
                {
                    var extension = __instance.def.GetModExtension<PlantExtension>();
                    if (extension != null && extension.transparencyWhenPawnOrItemIsBehind)
                    {
                        cachedTransparentablePlantsByExtensions[__instance] = extension;
                        if (!cachedTransparentablePlantsByMaps.TryGetValue(__instance.Map, out var list))
                        {
                            cachedTransparentablePlantsByMaps[__instance.Map] = list = new List<Thing>();
                        }
                        if (!list.Contains(__instance))
                        {
                            list.Add(__instance);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plant), "DeSpawn")]
        public class Thing_DeSpawn_Patch
        {
            private static void Prefix(Thing __instance)
            {
                if (MedievalOverhaulMod.settings.enableTreeTransparency && cachedTransparentablePlantsByExtensions.Remove(__instance) && __instance.Map != null
                    && cachedTransparentablePlantsByMaps.TryGetValue(__instance.Map, out var list))
                {
                    list.Remove(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "Position", MethodType.Setter)]
        public class Thing_Position_Patch
        {
            public static void Prefix(Thing __instance, out bool __state, IntVec3 value)
            {
                if (MedievalOverhaulMod.settings.enableTreeTransparency && BaseItemMatches(__instance) && __instance.Map != null && __instance.Position != value)
                {
                    __state = true;
                }
                else
                {
                    __state = false;
                }
            }
            public static void Postfix(Thing __instance, bool __state)
            {
                if (__state && cachedTransparentablePlantsByMaps.TryGetValue(__instance.Map, out var list))
                {
                    foreach (var plant in list)
                    {
                        if (__instance.Position.z > plant.Position.z && plant.Position.DistanceTo(__instance.Position) < 13)
                        {
                            RecheckTransparency(plant, __instance, plant.def.GetModExtension<PlantExtension>());
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Plant), "Graphic", MethodType.Getter)]
        public class Patch_Graphic_Postfix
        {
            private static void Postfix(Plant __instance, ref Graphic __result)
            {
                if (MedievalOverhaulMod.settings.enableTreeTransparency 
                    && cachedTransparentablePlantsByExtensions.TryGetValue(__instance, out var extension))
                {
                    var cells = GetTransparentCheckArea(__instance, extension);
                    bool anyItemsExistsInArea = cells.Any(x => HasItemsInCell(x, __instance.Map, extension));
                    if (anyItemsExistsInArea)
                    {
                        __result = GraphicDatabase.Get(__result.GetType(), "Transparent/" + __result.path, DankPyonDefOf.TransparentPlant.Shader,
                            __instance.def.graphicData.drawSize, __result.color, __result.colorTwo);
                        __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
                        lastCachedShaders[__instance] = DankPyonDefOf.TransparentPlant.Shader;

                    }
                    else
                    {
                        __result = GraphicDatabase.Get(__result.GetType(), __instance.def.graphicData.texPath, __instance.def.graphicData.shaderType.Shader,
                            __instance.def.graphicData.drawSize, __result.color, __result.colorTwo);
                        __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
                        lastCachedShaders[__instance] = __instance.def.graphicData.shaderType.Shader;
                    }
                }
            }
        }

        private static IEnumerable<Thing> MakeButcherProducts_Postfix(IEnumerable<Thing> __result, Pawn __instance, Pawn butcher, float efficiency)
        {
            Log.Message("Thing_MakeButcherProducts_FatAndBone_PostFix");
            foreach (var r in __result)
            {
                Log.Message("r: " + r);
                if (r.def.IsLeather)
                {
                    continue;
                }
                else
                {
                    Log.Message("YIeld: " + r);
                    yield return r;
                }
            }
            if (__instance.RaceProps.IsFlesh && __instance.RaceProps.meatDef != null)
            {
                bool boneFlag = true;
                bool fatFlag = true;

                var butcherProperties = ButcherProperties.Get(__instance.def);
                if (butcherProperties != null)
                {
                    boneFlag = butcherProperties.hasBone;
                    fatFlag = butcherProperties.hasFat;
                }

                if (boneFlag || fatFlag)
                {
                    int amount = Math.Max(1, (int)(GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) * 0.2f));
                    if (boneFlag)
                    {
                        Thing bone = ThingMaker.MakeThing(DankPyonDefOf.DankPyon_Bone, null);
                        bone.stackCount = amount;
                        yield return bone;
                    }
                    if (fatFlag)
                    {
                        Thing fat = ThingMaker.MakeThing(DankPyonDefOf.DankPyon_Fat, null);
                        fat.stackCount = amount;
                        yield return fat;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_ManTurret), nameof(JobDriver_ManTurret.FindAmmoForTurret))]
        public static class Patch_TryFindRandomShellDef
        {
            private static bool Prefix(Pawn pawn, Building_TurretGun gun, ref Thing __result)
            {
                if (gun.TryGetArtillery(out var group))
                {
                    StorageSettings allowedShellsSettings = pawn.IsColonist ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : RetrieveParentSettings(gun);
                    bool validator(Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t));
                    __result = GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(group), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false),
                        40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                    return false;
                }
                return true;
            }

            private static StorageSettings RetrieveParentSettings(Building_TurretGun gun)
            {
                return gun.gun.TryGetComp<CompChangeableProjectile>().GetParentStoreSettings();
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class ArtillerySearchGroup
    {
        private static readonly Dictionary<ThingDef, ThingRequestGroup> registeredArtillery = new Dictionary<ThingDef, ThingRequestGroup>();
        static ArtillerySearchGroup()
        {
            RegisterArtillery(DankPyonDefOf.DankPyon_Artillery_Trebuchet, ThingRequestGroup.Chunk);
        }

        public static bool RegisterArtillery(ThingDef def, ThingRequestGroup ammoGroup)
        {
            if (!registeredArtillery.ContainsKey(def))
            {
                registeredArtillery.Add(def, ammoGroup);
                return true;
            }
            return false;
        }

        public static bool TryGetArtillery(this Thing thing, out ThingRequestGroup group) => registeredArtillery.TryGetValue(thing.def, out group);
    }

    public class HediffFallRateMultiplier : DefModExtension
    {
        public HediffDef hediffDef;
    }

    [HarmonyPatch(typeof(HediffComp_SeverityPerDay), "SeverityChangePerDay")]
    [HarmonyPatch(typeof(HediffComp_Immunizable), "SeverityChangePerDay")]
    public class HediffComp_Immunizable_Patch
    {
        private static void Postfix(HediffComp_SeverityPerDay __instance, ref float __result)
        {
            if (HarmonyInstance.statMultipliers.TryGetValue(__instance.Def, out var stat))
            {
                __result *= __instance.Pawn.GetStatValue(stat);
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker_MarketValue), "CalculableRecipe")]
    public static class CalculableRecipe_Patch
    {
        private static bool Prefix(BuildableDef def)
        {
            if (def is ThingDef thingDef && thingDef.IsChunk())
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef), "CanBeResearchedAt")]
    public static class ResearchProjectDef_CanBeResearchedAt_Patch
    {
        public static void Postfix(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus, ResearchProjectDef __instance, ref bool __result)
        {
            if (__result)
            {
                var fuelComp = bench.GetComp<CompRefuelable>();
                if (fuelComp != null && !fuelComp.HasFuel)
                {
                    __result = false;
                }
            }
        }
    }

    public class ProcessorExtension : DefModExtension
    {
        public bool outputOnlyButcherProduct;
    }
    [HarmonyPatch(typeof(CompProcessor), "TakeOutProduct")]
    public static class CompProcessor_TakeOutProduct_Patch
    {
        public static bool Prefix(CompProcessor __instance, ref Thing __result, ActiveProcess activeProcess)
        {
            var extension = activeProcess.processDef.GetModExtension<ProcessorExtension>();
            if (extension != null)
            {
                if (extension.outputOnlyButcherProduct)
                {
                    foreach (var thing in activeProcess.ingredientThings)
                    {
                        if (thing.def.butcherProducts.Any())
                        {
                            var thingDefCount = thing.def.butcherProducts.FirstOrDefault();
                            __result = TakeOutButcherProduct(__instance, thingDefCount, activeProcess);
                            return false;
                        }
                        else if (thing.def == DankPyonDefOf.DankPyon_Hide_HideGeneric)
                        {
                            var comp = thing.TryGetComp<CompGenericHide>();

                        }
                    }

                }
            }
            return true;
        }

        public static Thing TakeOutButcherProduct(CompProcessor __instance, ThingDefCountClass thingDefCount, ActiveProcess activeProcess)
        {
            Thing thing = null;
            if (!activeProcess.Ruined)
            {
                thing = ThingMaker.MakeThing(thingDefCount.thingDef);
                thing.stackCount = thingDefCount.count;
                CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
                List<ThingDef> list = new List<ThingDef>();
                foreach (Thing ingredientThing in activeProcess.ingredientThings)
                {
                    List<ThingDef> list2 = ingredientThing.TryGetComp<CompIngredients>()?.ingredients;
                    if (!list2.NullOrEmpty())
                    {
                        list.AddRange(list2);
                    }
                }
                if (compIngredients != null && !list.NullOrEmpty())
                {
                    compIngredients.ingredients.AddRange(list);
                }
                if (activeProcess.processDef.usesQuality)
                {
                    thing.TryGetComp<CompQuality>()?.SetQuality(activeProcess.CurrentQuality, ArtGenerationContext.Colony);
                }
                foreach (BonusOutput bonusOutput in activeProcess.processDef.bonusOutputs)
                {
                    if (!Rand.Chance(bonusOutput.chance))
                    {
                        continue;
                    }
                    int num = GenMath.RoundRandom((float)activeProcess.ingredientCount * activeProcess.processDef.capacityFactor / (float)__instance.Props.capacity * (float)bonusOutput.amount);
                    if (num <= 0)
                    {
                        continue;
                    }
                    if (bonusOutput.thingDef.race != null)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(bonusOutput.thingDef.race.AnyPawnKind, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: true)), __instance.parent.Position, __instance.parent.Map);
                        }
                    }
                    else
                    {
                        Thing thing2 = ThingMaker.MakeThing(bonusOutput.thingDef);
                        thing2.stackCount = num;
                        GenPlace.TryPlaceThing(thing2, __instance.parent.Position, __instance.parent.Map, ThingPlaceMode.Near);
                    }
                }
            }
            foreach (Thing ingredientThing2 in activeProcess.ingredientThings)
            {
                __instance.innerContainer.Remove(ingredientThing2);
                ingredientThing2.Destroy();
            }
            __instance.activeProcesses.Remove(activeProcess);
            if (Rand.Chance(activeProcess.processDef.destroyChance * (float)activeProcess.ingredientCount * activeProcess.processDef.capacityFactor / (float)__instance.Props.capacity))
            {
                if (PF_Settings.replaceDestroyedProcessors)
                {
                    GenConstruct.PlaceBlueprintForBuild_NewTemp(__instance.parent.def, __instance.parent.Position, __instance.parent.Map, __instance.parent.Rotation, Faction.OfPlayer, null);
                }
                __instance.parent.Destroy();
                return thing;
            }
            if (__instance.Empty)
            {
                __instance.GraphicChange(toEmpty: true);
            }
            if (!__instance.activeProcesses.Any((ActiveProcess x) => x.processDef.usesQuality))
            {
                __instance.emptyNow = false;
            }
            return thing;
        }
    }

}
