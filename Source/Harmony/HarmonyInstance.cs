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
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {
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
                if (pawnKindDef.RaceProps.Animal && pawnKindDef.race.race.leatherDef != null && pawnKindDef.lifeStages != null 
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

        private static IEnumerable<Thing> MakeButcherProducts_Postfix(IEnumerable<Thing> __result, Pawn __instance, Pawn butcher, float efficiency)
        {
            foreach (var r in __result)
            {
                if (r.def.IsLeather)
                {
                    continue;
                }
                else
                {
                    var comp = r.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        comp.pawnSource = __instance.def;
                        var leatherDef = comp.pawnSource.race.leatherDef;
                        comp.leatherAmount = GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.LeatherAmount) * efficiency);
                        var leatherCost = leatherDef.GetStatValueAbstract(StatDefOf.MarketValue);
                        comp.marketValue = (int)((int)(comp.leatherAmount * leatherCost) - ((comp.leatherAmount * leatherCost) * 0.2f));
                        if (r is HideGeneric hideGeneric)
                        {
                            hideGeneric.drawColorOverride = leatherDef.graphicData.color;
                        }
                    }
                    yield return r;
                }
            }

            var additionalButcherOptions = __instance.def.GetModExtension<AdditionalButcherProducts>();
            if (additionalButcherOptions != null)
            {
                foreach (var option in additionalButcherOptions.butcherOptions)
                {
                    if (Rand.Chance(option.chance))
                    {
                        Thing bone = ThingMaker.MakeThing(option.thingDef, null);
                        bone.stackCount = option.amount.RandomInRange;
                        yield return bone;
                    }
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

        [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
        public static class StatExtension_GetStatValue_Patch
        {
            private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
            {
                if (stat == StatDefOf.MarketValue)
                {
                    var comp = thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        __result = comp.marketValue;
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
                        var comp = thing.TryGetComp<CompGenericHide>();
                        if (comp != null)
                        {
                            var thingDefCount = new ThingDefCountClass
                            {
                                count = comp.leatherAmount,
                                thingDef = comp.pawnSource.race.leatherDef
                            };
                            __result = TakeOutButcherProduct(__instance, thingDefCount, activeProcess);
                            return false;
                        }
                        else if (thing.def.butcherProducts?.Any() ?? false)
                        {
                            var thingDefCount = thing.def.butcherProducts.FirstOrDefault();
                            __result = TakeOutButcherProduct(__instance, thingDefCount, activeProcess);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public static void Postfix(CompProcessor __instance, ref Thing __result, ActiveProcess activeProcess)
        {
            if (activeProcess.processDef == DankPyonDefOf.DankPyon_RawHidesProcess)
            {
                foreach (var thing in activeProcess.ingredientThings)
                {
                    var comp = thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        __result.stackCount = comp.leatherAmount;
                    }
                }
            }
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
